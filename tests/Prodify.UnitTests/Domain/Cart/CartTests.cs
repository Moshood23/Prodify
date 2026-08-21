using Prodify.Domain.Cart.Entities;

namespace Prodify.UnitTests.Domain.Cart;

public class CartTests
{
    [Fact]
    public void CreateForCustomer_SetsCustomerId()
    {
        var customerId = Guid.NewGuid();
        var cart = Prodify.Domain.Cart.Entities.Cart.CreateForCustomer(customerId);

        Assert.Equal(customerId, cart.CustomerId);
        Assert.Null(cart.SessionId);
    }

    [Fact]
    public void CreateForGuest_SetsSessionId()
    {
        var cart = Prodify.Domain.Cart.Entities.Cart.CreateForGuest("session-123");

        Assert.Equal("session-123", cart.SessionId);
        Assert.Null(cart.CustomerId);
    }

    [Fact]
    public void AddItem_NewVariant_AddsToItems()
    {
        var cart = Prodify.Domain.Cart.Entities.Cart.CreateForGuest("session-123");
        cart.AddItem(Guid.NewGuid(), 2, 100m);

        Assert.Single(cart.Items);
        Assert.Equal(200m, cart.Total);
    }

    [Fact]
    public void AddItem_ExistingVariant_MergesQuantity()
    {
        var cart = Prodify.Domain.Cart.Entities.Cart.CreateForGuest("session-123");
        var variantId = Guid.NewGuid();

        cart.AddItem(variantId, 2, 100m);
        cart.AddItem(variantId, 3, 100m);

        Assert.Single(cart.Items);
        Assert.Equal(5, cart.Items.First().Quantity);
    }

    [Fact]
    public void RemoveItem_RemovesFromCollection()
    {
        var cart = Prodify.Domain.Cart.Entities.Cart.CreateForGuest("session-123");
        cart.AddItem(Guid.NewGuid(), 2, 100m);
        var itemId = cart.Items.First().Id;

        cart.RemoveItem(itemId);

        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var cart = Prodify.Domain.Cart.Entities.Cart.CreateForGuest("session-123");
        cart.AddItem(Guid.NewGuid(), 2, 100m);
        cart.AddItem(Guid.NewGuid(), 1, 50m);

        cart.Clear();

        Assert.Empty(cart.Items);
    }

    [Fact]
    public void AssignToCustomer_ClearsSessionId()
    {
        var cart = Prodify.Domain.Cart.Entities.Cart.CreateForGuest("session-123");
        var customerId = Guid.NewGuid();

        cart.AssignToCustomer(customerId);

        Assert.Equal(customerId, cart.CustomerId);
        Assert.Null(cart.SessionId);
    }
}