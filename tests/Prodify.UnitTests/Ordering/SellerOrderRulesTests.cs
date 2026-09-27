using Prodify.Application.Ordering.Common;
using Prodify.Application.Ordering.SellerOrders.Common;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Ordering.ValueObjects;
using Prodify.Domain.Shipping.Entities;

namespace Prodify.UnitTests.Ordering;

public class SellerOrderRulesTests
{
    private static Order PlaceOrder(PaymentMethod paymentMethod) =>
        Order.Place(
            Guid.NewGuid(),
            OrderAddress.Create("Chi Oke", "5 Allen Ave", "Ikeja", "Lagos", "Nigeria", "08031234567"),
            new[] { (Guid.NewGuid(), Guid.NewGuid(), "Kettle", 1, Money.Create(18_500m)) },
            paymentMethod);

    [Fact]
    public void UnpaidCardOrder_IsNotReadyForTheSeller()
    {
        var order = PlaceOrder(PaymentMethod.Card);

        Assert.False(SellerOrderRules.IsReadyForSeller(order));
        Assert.False(SellerOrderRules.CanConfirm(order.SellerOrders.Single(), order));
    }

    [Fact]
    public void PayOnDeliveryOrder_CanBeConfirmedStraightAway()
    {
        var order = PlaceOrder(PaymentMethod.PayOnDelivery);

        Assert.True(SellerOrderRules.CanConfirm(order.SellerOrders.Single(), order));
    }

    [Fact]
    public void PaidOrder_CanBeConfirmedButNotCancelled()
    {
        var order = PlaceOrder(PaymentMethod.Card);
        order.MarkAsPaid(Guid.NewGuid());
        var sellerOrder = order.SellerOrders.Single();

        Assert.True(SellerOrderRules.CanConfirm(sellerOrder, order));
        Assert.False(SellerOrderRules.CanCancel(sellerOrder, order));
    }

    [Fact]
    public void Steps_FollowConfirmPackShipDeliver()
    {
        var order = PlaceOrder(PaymentMethod.PayOnDelivery);
        var sellerOrder = order.SellerOrders.Single();

        Assert.False(SellerOrderRules.CanPack(sellerOrder));

        sellerOrder.TransitionTo(SellerOrderStatus.Confirmed);
        Assert.True(SellerOrderRules.CanPack(sellerOrder));
        Assert.True(SellerOrderRules.CanCancel(sellerOrder, order));

        sellerOrder.TransitionTo(SellerOrderStatus.Packed);
        Assert.True(SellerOrderRules.CanShip(sellerOrder));
        Assert.False(SellerOrderRules.CanCancel(sellerOrder, order));

        sellerOrder.TransitionTo(SellerOrderStatus.Shipped);
        Assert.True(SellerOrderRules.CanDeliver(sellerOrder));
    }

    [Fact]
    public void Customer_CannotCancelOnceASellerHasPacked()
    {
        var order = PlaceOrder(PaymentMethod.PayOnDelivery);
        var sellerOrder = order.SellerOrders.Single();
        Assert.True(OrderRules.CanBeCancelled(order));

        sellerOrder.TransitionTo(SellerOrderStatus.Confirmed);
        sellerOrder.TransitionTo(SellerOrderStatus.Packed);

        Assert.False(OrderRules.CanBeCancelled(order));
    }

    [Fact]
    public void Shipment_CanBeShippedWithoutATrackingNumber()
    {
        var shipment = Shipment.Create(Guid.NewGuid(), new[] { (Guid.NewGuid(), 1) });

        shipment.Ship("Own rider", null);

        Assert.Equal(ShipmentStatus.Shipped, shipment.Status);
        Assert.Null(shipment.TrackingNumber);
        Assert.Equal("Own rider", shipment.Carrier);
    }
}