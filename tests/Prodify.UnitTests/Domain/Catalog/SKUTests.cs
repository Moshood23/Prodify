using Prodify.Domain.Catalog.ValueObjects;

namespace Prodify.UnitTests.Domain.Catalog;

public class SKUTests
{
    [Fact]
    public void Create_WithValidValue_NormalizesToUppercase()
    {
        var sku = SKU.Create("abc-123");

        Assert.Equal("ABC-123", sku.Value);
    }

    [Fact]
    public void Create_WithEmptyValue_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SKU.Create(string.Empty));
    }

    [Fact]
    public void Create_WithWhitespace_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SKU.Create("   "));
    }

    [Fact]
    public void TwoSKUs_WithSameValue_AreEqual()
    {
        var sku1 = SKU.Create("abc-123");
        var sku2 = SKU.Create("ABC-123");

        Assert.Equal(sku1, sku2);
    }
}