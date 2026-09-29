using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Ordering.ValueObjects;
using Prodify.Domain.Payments.Entities;

namespace Prodify.Application.Ordering.Common;

public static class OrderPayments
{
    // Pay on delivery: once every part of the order that wasn't cancelled has
    // been delivered, the cash has been collected, so the order is paid.
    // Needs the order's seller orders and their items loaded.
    public static void RecordCashOnDeliveryIfComplete(this IApplicationDbContext context, Order order)
    {
        if (order.PaymentMethod != PaymentMethod.PayOnDelivery || order.IsPaid || order.Status != OrderStatus.Delivered)
            return;

        // The delivered parts plus the delivery fee.
        var collected = order.SellerOrders
            .Where(so => so.Status == SellerOrderStatus.Delivered)
            .Select(so => so.Total)
            .Aggregate(Money.Create(order.DeliveryFee), (sum, next) => sum.Add(next));

        var payment = Payment.Create(order.Id, collected);
        var attempt = payment.StartAttempt();
        payment.CompleteAttempt(attempt.Id, $"CASH-{order.OrderNumber.Value}");
        context.Add(payment);

        order.MarkAsPaid(payment.Id);
    }
}
