using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.Common;

public static class OrderRules
{
    // Only before any seller has packed their part. Paid orders would need a refund, which isn't built yet.
    public static bool CanBeCancelled(Order order) =>
        (order.Status is OrderStatus.Pending or OrderStatus.Confirmed)
        && !order.IsPaid
        && order.SellerOrders.All(so => so.Status is SellerOrderStatus.Pending or SellerOrderStatus.Confirmed or SellerOrderStatus.Cancelled);
}