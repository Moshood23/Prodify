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

        // The delivered parts plus the delivery fee, less their share of any voucher discount
        // and less any store credit still on the order.
        var delivered = order.SellerOrders.Where(so => so.Status == SellerOrderStatus.Delivered).ToList();
        var value = delivered.Sum(so => so.Total.Amount) + order.DeliveryFee - order.DiscountShare(delivered);
        var collected = Money.Create(Math.Max(0, value - order.CreditLeft));

        var payment = Payment.Create(order.Id, collected);
        var attempt = payment.StartAttempt();
        payment.CompleteAttempt(attempt.Id, $"CASH-{order.OrderNumber.Value}");
        context.Add(payment);

        order.MarkAsPaid(payment.Id);
    }
}
