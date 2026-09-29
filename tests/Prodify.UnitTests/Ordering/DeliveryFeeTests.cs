using Prodify.Application.Ordering.Common;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Ordering.ValueObjects;
using Prodify.Domain.Shipping.Entities;

namespace Prodify.UnitTests.Ordering;

public class DeliveryFeeTests
{
    private static readonly OrderAddress Address =
        OrderAddress.Create("Chi Oke", "5 Allen Ave", "Ikeja", "Lagos", "Nigeria", "08031234567");

    // Two sellers: a 10,000 kettle and a 5,000 fan, delivered for 2,500.
    private static Order PlaceTwoSellerOrder() =>
        Order.Place(
            Guid.NewGuid(),
            Address,
            new[]
            {
                (Guid.NewGuid(), Guid.NewGuid(), "Kettle", 1, Money.Create(10_000m)),
                (Guid.NewGuid(), Guid.NewGuid(), "Fan", 1, Money.Create(5_000m)),
            },
            PaymentMethod.Card,
            deliveryFee: 2_500m);

    [Fact]
    public void Total_IsItemsPlusDelivery()
    {
        var order = PlaceTwoSellerOrder();

        Assert.Equal(15_000m, order.ItemsTotal.Amount);
        Assert.Equal(17_500m, order.Total.Amount);
    }

    [Fact]
    public void Refund_ForOnePart_LeavesTheDeliveryFee()
    {
        var order = PlaceTwoSellerOrder();
        var kettle = order.SellerOrders.First();

        Assert.Equal(10_000m, OrderRefunds.RefundFor(order, new[] { kettle }));
    }

    [Fact]
    public void Refund_ForEverything_IncludesTheDeliveryFee()
    {
        var order = PlaceTwoSellerOrder();

        Assert.Equal(17_500m, OrderRefunds.RefundFor(order, order.SellerOrders.ToList()));
    }

    [Fact]
    public void Refund_ForTheLastPart_IncludesTheDeliveryFee()
    {
        var order = PlaceTwoSellerOrder();
        var (kettle, fan) = (order.SellerOrders.First(), order.SellerOrders.Last());
        kettle.TransitionTo(SellerOrderStatus.Cancelled, "Out of stock");

        Assert.Equal(7_500m, OrderRefunds.RefundFor(order, new[] { fan }));
    }

    [Fact]
    public void Place_WithNegativeDeliveryFee_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Order.Place(
            Guid.NewGuid(), Address, new[] { (Guid.NewGuid(), Guid.NewGuid(), "Kettle", 1, Money.Create(10_000m)) },
            PaymentMethod.Card, deliveryFee: -1m));
    }

    [Fact]
    public void DeliveryFee_MustBeBetweenZeroAndTheMaximum()
    {
        var fee = DeliveryFee.Create("Lagos", 2_500m);

        fee.ChangeAmount(0m);
        Assert.Equal(0m, fee.Amount);
        Assert.Throws<ArgumentOutOfRangeException>(() => fee.ChangeAmount(-100m));
        Assert.Throws<ArgumentOutOfRangeException>(() => fee.ChangeAmount(DeliveryFee.MaxAmount + 1));
    }
}
