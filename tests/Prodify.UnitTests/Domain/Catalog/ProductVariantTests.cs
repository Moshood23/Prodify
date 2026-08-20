using Prodify.Domain.Catalog.Entities;

namespace Prodify.UnitTests.Domain.Catalog;

public class ProductVariantTests
{
    [Fact]
    public void Create_WithValidData_CreatesVariant()
    {
        var variant = ProductVariant.Create(Guid.NewGuid(), "sku-123", 100m);

        Assert.Equal("SKU-123", variant.SKU.Value);
        Assert.Equal(100m, variant.Price);
        Assert.True(variant.IsActive);
    }

    [Fact]
    public void Create_WithNegativePrice_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            ProductVariant.Create(Guid.NewGuid(), "sku-123", -10m));
    }

    [Fact]
    public void Create_WithEmptyProductId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            ProductVariant.Create(Guid.Empty, "sku-123", 100m));
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_UpdatesPrice()
    {
        var variant = ProductVariant.Create(Guid.NewGuid(), "sku-123", 100m);
        variant.UpdatePrice(150m);

        Assert.Equal(150m, variant.Price);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var variant = ProductVariant.Create(Guid.NewGuid(), "sku-123", 100m);
        variant.Deactivate();

        Assert.False(variant.IsActive);
    }
}