using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.SellerOrders.Common;

// What a seller can do with their part of an order, step by step:
// Pending -> Confirmed -> Packed -> Shipped -> Delivered, or Cancelled before packing.
public static class SellerOrderRules
{
    // A card order is only real once it is paid; pay-on-delivery orders are ready straight away.
    public static bool IsReadyForSeller(Order order) =>
        order.PaymentMethod == PaymentMethod.PayOnDelivery || order.IsPaid;

    public static bool CanConfirm(SellerOrder sellerOrder, Order order) =>
        sellerOrder.Status == SellerOrderStatus.Pending && IsReadyForSeller(order);

    public static bool CanPack(SellerOrder sellerOrder) => sellerOrder.Status == SellerOrderStatus.Confirmed;

    public static bool CanShip(SellerOrder sellerOrder) => sellerOrder.Status == SellerOrderStatus.Packed;

    public static bool CanDeliver(SellerOrder sellerOrder) => sellerOrder.Status == SellerOrderStatus.Shipped;

    // Paid orders would need a refund, which isn't built yet.
    public static bool CanCancel(SellerOrder sellerOrder, Order order) =>
        sellerOrder.Status is SellerOrderStatus.Pending or SellerOrderStatus.Confirmed && !order.IsPaid;
}