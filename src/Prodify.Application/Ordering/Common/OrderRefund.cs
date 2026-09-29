using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Ordering.ValueObjects;
using Prodify.Domain.Payments.Entities;

namespace Prodify.Application.Ordering.Common;

public static class OrderRefunds
{
    // What to send back when these parts are cancelled: their items, plus the
    // delivery fee if nothing else in the order is left to deliver.
    // Call it before the parts are marked as cancelled.
    public static decimal RefundFor(Order order, IReadOnlyCollection<SellerOrder> cancelling)
    {
        var items = cancelling.Sum(so => so.Total.Amount);
        var nothingLeft = order.SellerOrders.All(so => cancelling.Contains(so) || so.Status == SellerOrderStatus.Cancelled);

        return nothingLeft ? items + order.DeliveryFee : items;
    }

    // Card orders that were paid get the money for cancelled parts sent back.
    // Unpaid orders and pay-on-delivery orders (paid only once delivered) have nothing to refund.
    public static async Task RefundIfPaidAsync(
        this IApplicationDbContext context,
        IPaymentService paymentService,
        Order order,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (!order.IsPaid || order.PaymentMethod != PaymentMethod.Card || amount <= 0)
            return;

        var payment = await context.Payments
            .Include(p => p.Attempts)
            .FirstOrDefaultAsync(p => p.OrderId == order.Id
                && (p.Status == PaymentStatus.Succeeded || p.Status == PaymentStatus.PartiallyRefunded || p.Status == PaymentStatus.Refunded),
                cancellationToken);

        if (payment is null || !payment.IsCaptured || payment.GatewayReference is null)
            throw new BusinessRuleException("The payment for this order could not be found, so it can't be refunded.");

        // Never send back more than is left on the payment.
        var refund = Math.Min(amount, payment.Amount.Amount - payment.RefundedAmount);
        if (refund <= 0)
            return;

        var result = await paymentService.RefundAsync(payment.GatewayReference, Money.Create(refund), cancellationToken);
        if (!result.Succeeded)
            throw new BusinessRuleException($"The refund failed: {result.FailureReason ?? "unknown error"}. Nothing was cancelled.");

        payment.RecordRefund(refund);
    }
}
