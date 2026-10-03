using Prodify.Application.Cart.Common;

namespace Prodify.UnitTests.Cart;

public class CartPricingTests
{
    private static CartVariantInfo Variant(Guid id, decimal price, bool available = true, int stock = 10) =>
        new(id, Guid.NewGuid(), Guid.NewGuid(), "Fan", null, price, null, available, stock);

    [Fact]
    public void Build_UsesTodaysPrice_AndSkipsUnavailableItemsInTotal()
    {
        var fan = Guid.NewGuid();
        var gone = Guid.NewGuid();
        var lines = new[]
        {
            new CartLine(Guid.NewGuid(), fan, 2, 25_000m),
            new CartLine(Guid.NewGuid(), gone, 1, 5_000m),
        };
        var variants = new Dictionary<Guid, CartVariantInfo> { [fan] = Variant(fan, 30_000m) };

        var cart = CartPricing.Build(null, lines, variants);

        Assert.Equal(60_000m, cart.Total);
        Assert.Equal(3, cart.ItemCount);
        Assert.True(cart.HasProblems);
        Assert.Equal("Product no longer available", cart.Items.Single(i => i.ProductVariantId == gone).ProductName);
    }

    [Fact]
    public void Build_FlagsItemsWithTooLittleStock()
    {
        var fan = Guid.NewGuid();
        var variants = new Dictionary<Guid, CartVariantInfo> { [fan] = Variant(fan, 30_000m, stock: 1) };

        var cart = CartPricing.Build(null, new[] { new CartLine(fan, fan, 3, 30_000m) }, variants);

        Assert.False(cart.Items[0].InStock);
        Assert.True(cart.HasProblems);
    }
}
