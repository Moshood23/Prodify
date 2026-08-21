using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.UnitTests.Domain.Ordering;

public class OrderTests
{
    private static OrderAddress CreateTestAddress() =>
        OrderAddress.Create("John Doe", "123 Main St", "Lagos", "Lagos", "Nigeria", "08012345678");

    private static (Guid SellerId, Guid ProductVariantId, string ProductName, int Quantity, Money UnitPrice) CreateLineItem(Guid sellerId) =>
        (sellerId, Guid.NewGuid(), "Test Product", 2, Money.Create(500m));

    [Fact]
    public void Place_WithSingleSeller_CreatesOneSellerOrder()
    {
        var sellerId = Guid.NewGuid();
        var lineItems = new[] { CreateLineItem(sellerId) };

        var order = Order.Place(Guid.NewGuid(), CreateTestAddress(), lineItems);

        Assert.Single(order.SellerOrders);
        Assert.Single(order.Items);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void Place_WithMultipleSellers_SplitsIntoMultipleSellerOrders()
    {
        var lineItems = new[]
        {
            CreateLineItem(Guid.NewGuid()),
            CreateLineItem(Guid.NewGuid())
        };

        var order = Order.Place(Guid.NewGuid(), CreateTestAddress(), lineItems);

        Assert.Equal(2, order.SellerOrders.Count);
    }

    [Fact]
    public void Place_WithNoItems_ThrowsInvalidOperationException()
    {
        var lineItems = Array.Empty<(Guid, Guid, string, int, Money)>();

        Assert.Throws<InvalidOperationException>(() =>
            Order.Place(Guid.NewGuid(), CreateTestAddress(), lineItems));
    }

    [Fact]
    public void MarkAsPaid_SetsIsPaidTrue()
    {
        var order = Order.Place(Guid.NewGuid(), CreateTestAddress(), new[] { CreateLineItem(Guid.NewGuid()) });

        order.MarkAsPaid(Guid.NewGuid());

        Assert.True(order.IsPaid);
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_ThrowsInvalidOperationException()
    {
        var order = Order.Place(Guid.NewGuid(), CreateTestAddress(), new[] { CreateLineItem(Guid.NewGuid()) });
        order.MarkAsPaid(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => order.MarkAsPaid(Guid.NewGuid()));
    }

    [Fact]
    public void Cancel_WhenPending_CancelsAllSellerOrders()
    {
        var order = Order.Place(Guid.NewGuid(), CreateTestAddress(), new[] { CreateLineItem(Guid.NewGuid()) });

        order.Cancel("Customer requested");

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Status_WhenAllSellerOrdersDelivered_ReturnsDelivered()
    {
        var order = Order.Place(Guid.NewGuid(), CreateTestAddress(), new[] { CreateLineItem(Guid.NewGuid()) });
        var sellerOrder = order.SellerOrders.First();

        sellerOrder.TransitionTo(SellerOrderStatus.Confirmed);
        sellerOrder.TransitionTo(SellerOrderStatus.Packed);
        sellerOrder.TransitionTo(SellerOrderStatus.Shipped);
        order.MarkSellerOrderDelivered(sellerOrder.Id);

        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void Total_SumsAllItemSubtotals()
    {
        var order = Order.Place(Guid.NewGuid(), CreateTestAddress(), new[] { CreateLineItem(Guid.NewGuid()) });

        Assert.Equal(1000m, order.Total.Amount);
    }
}