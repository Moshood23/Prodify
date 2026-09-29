using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.Common;

public static class OrderRules
{
    // Customers: only before any seller has packed their part. Paid card orders are refunded.
    public static bool CanBeCancelled(Order order) =>
        (order.Status is OrderStatus.Pending or OrderStatus.Confirmed)
        && order.SellerOrders.All(so => so.Status is SellerOrderStatus.Pending or SellerOrderStatus.Confirmed or SellerOrderStatus.Cancelled);

    // A part that hasn't left the seller yet.
    public static bool IsStoppable(SellerOrder sellerOrder) =>
        sellerOrder.Status is SellerOrderStatus.Pending or SellerOrderStatus.Confirmed or SellerOrderStatus.Packed;

    // Admins: any time before something was shipped. Packed parts can still be stopped.
    public static bool CanAdminCancel(Order order) =>
        order.SellerOrders.Any(IsStoppable)
        && order.SellerOrders.All(so => IsStoppable(so) || so.Status == SellerOrderStatus.Cancelled);
}