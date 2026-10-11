using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Ordering.Common;
using Prodify.Domain.Customers.Entities;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Returns.Entities;

namespace Prodify.Application.Returns.Common;

public static class ReturnRefunds
{
    // Returns can be asked for while the seller's money for the part is still held: 7 days after delivery.
    public static TimeSpan Window => Domain.Payouts.Entities.SellerEarning.HoldPeriod;

    // Accepting or approving a return: the money goes back and the seller's earning goes down.
    // Card orders: back to the card first, then any store credit that paid for the order.
    // Cash orders: the whole refund as store credit.
    public static async Task<RefundSplit> RefundReturnAsync(
        this IApplicationDbContext context,
        IPaymentService paymentService,
        ReturnRequest request,
        string? adminNote,
        CancellationToken cancellationToken)
    {
        if (!request.IsOpen)
            throw new BusinessRuleException($"This return is already {request.Status.ToString().ToLowerInvariant()}.");

        var order = await context.Orders
            .Include(o => o.Items)
            .Include(o => o.SellerOrders)
            .FirstAsync(o => o.Id == request.OrderId, cancellationToken);

        var value = request.RefundAmount;
        decimal toCard = 0;
        decimal toCredit;

        if (order.PaymentMethod == PaymentMethod.Card)
        {
            toCard = await OrderRefunds.RefundToCardAsync(context, paymentService, order, value, cancellationToken);
            toCredit = Math.Min(value - toCard, order.CreditLeft);
            if (toCredit > 0)
                order.ReturnCredit(toCredit);
        }
        else
        {
            // The cash was handed over at the door, so it comes back as store credit.
            toCredit = value;
            var fromCredit = Math.Min(value, order.CreditLeft);
            if (fromCredit > 0)
                order.ReturnCredit(fromCredit);
        }

        if (toCredit > 0)
            context.Add(StoreCreditEntry.Add(order.CustomerId, toCredit, $"Return from order {order.OrderNumber.Value}", order.Id));

        var earning = await context.SellerEarnings.FirstOrDefaultAsync(e => e.SellerOrderId == request.SellerOrderId, cancellationToken);
        if (earning is not null)
        {
            earning.RecordReturn(Math.Min(request.ItemsValue, earning.Sales));
            earning.ReleaseHold();
        }

        request.MarkRefunded(toCard, toCredit, adminNote);
        return new RefundSplit(toCard, toCredit);
    }

    // The return is over without a refund: the seller's money is free again.
    public static async Task ReleaseEarningAsync(this IApplicationDbContext context, ReturnRequest request, CancellationToken cancellationToken)
    {
        var earning = await context.SellerEarnings.FirstOrDefaultAsync(e => e.SellerOrderId == request.SellerOrderId, cancellationToken);
        earning?.ReleaseHold();
    }
}
