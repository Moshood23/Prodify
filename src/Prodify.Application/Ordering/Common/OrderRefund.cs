using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Customers.Entities;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Ordering.ValueObjects;
using Prodify.Domain.Payments.Entities;

namespace Prodify.Application.Ordering.Common;

// What a customer got back: to their card, and as store credit.
public record RefundSplit(decimal ToCard, decimal ToCredit)
{
    public static readonly RefundSplit None = new(0, 0);

    public decimal Total => ToCard + ToCredit;
}

public static class OrderRefunds
{
    // What to send back when these parts are cancelled: their items less their share of any
    // voucher discount, plus the delivery fee if nothing else in the order is left to deliver.
    // Call it before the parts are marked as cancelled.
    public static decimal RefundFor(Order order, IReadOnlyCollection<SellerOrder> cancelling)
    {
        var items = cancelling.Sum(so => so.Total.Amount) - order.DiscountShare(cancelling);
        var nothingLeft = order.SellerOrders.All(so => cancelling.Contains(so) || so.Status == SellerOrderStatus.Cancelled);

        return nothingLeft ? items + order.DeliveryFee : items;
    }

    // What the parts still going ahead are worth once these are cancelled (0 if none are left).
    public static decimal ValueLeftAfter(Order order, IReadOnlyCollection<SellerOrder> cancelling)
    {
        var left = order.SellerOrders
            .Where(so => !cancelling.Contains(so) && so.Status != SellerOrderStatus.Cancelled)
            .ToList();

        return left.Count == 0 ? 0 : left.Sum(so => so.Total.Amount) - order.DiscountShare(left) + order.DeliveryFee;
    }

    // Gives back the money for parts being cancelled. Call it before the parts are marked as cancelled.
    // Card money goes back to the card first; whatever was paid with store credit goes back as credit.
    // Pay on delivery orders haven't paid cash yet, so only credit the remaining parts can't use goes back.
    public static async Task<RefundSplit> RefundCancelledPartsAsync(
        this IApplicationDbContext context,
        IPaymentService paymentService,
        Order order,
        IReadOnlyCollection<SellerOrder> cancelling,
        CancellationToken cancellationToken)
    {
        var value = RefundFor(order, cancelling);
        var toCard = await RefundToCardAsync(context, paymentService, order, value, cancellationToken);

        var creditBack = order.PaymentMethod == PaymentMethod.PayOnDelivery && !order.IsPaid
            ? Math.Max(0, order.CreditLeft - ValueLeftAfter(order, cancelling))
            : order.CreditLeft;
        var toCredit = Math.Min(value - toCard, creditBack);

        if (toCredit > 0)
        {
            order.ReturnCredit(toCredit);
            context.Add(StoreCreditEntry.Add(order.CustomerId, toCredit, $"Cancelled from order {order.OrderNumber.Value}", order.Id));
        }

        return new RefundSplit(toCard, toCredit);
    }

    // Card orders that were paid get money sent back to the card, up to what is left on the payment.
    // Unpaid orders and pay-on-delivery orders (paid only once delivered) have nothing to refund.
    // Returns how much was refunded (0 if nothing).
    public static async Task<decimal> RefundToCardAsync(
        IApplicationDbContext context,
        IPaymentService paymentService,
        Order order,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (!order.IsPaid || order.PaymentMethod != PaymentMethod.Card || amount <= 0)
            return 0;

        var payment = await context.Payments
            .Include(p => p.Attempts)
            .FirstOrDefaultAsync(p => p.OrderId == order.Id
                && (p.Status == PaymentStatus.Succeeded || p.Status == PaymentStatus.PartiallyRefunded || p.Status == PaymentStatus.Refunded),
                cancellationToken);

        if (payment is null || !payment.IsCaptured || payment.GatewayReference is null)
            throw new BusinessRuleException("The payment for this order could not be found, so it can't be refunded.");

        // Never send back more than is left on the payment (store credit may have paid the rest).
        var refund = Math.Min(amount, payment.Amount.Amount - payment.RefundedAmount);
        if (refund <= 0)
            return 0;

        var result = await paymentService.RefundAsync(payment.GatewayReference, Money.Create(refund), cancellationToken);
        if (!result.Succeeded)
            throw new BusinessRuleException($"The refund failed: {result.FailureReason ?? "unknown error"}. Nothing was cancelled.");

        payment.RecordRefund(refund);
        return refund;
    }
}
