using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.Common;

public static class OrderRules
{
    // Only before anything has shipped. Paid orders would need a refund, which isn't built yet.
    public static bool CanBeCancelled(Order order) =>
        (order.Status is OrderStatus.Pending or OrderStatus.Confirmed) && !order.IsPaid;
}