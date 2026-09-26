using Prodify.Domain.Catalog.Entities;

namespace Prodify.UnitTests.Domain.Catalog;

public class ProductTests
{
    [Fact]
    public void Create_WithValidName_CreatesProduct()
    {
        var product = Product.Create("Test Product", "A description", Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal("Test Product", product.Name);
        Assert.True(product.IsActive);
        Assert.Single(product.DomainEvents);
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Product.Create(string.Empty, null, Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithEmptySellerId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Product.Create("Test Product", null, Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public void AddAttribute_WithDuplicateName_ThrowsInvalidOperationException()
    {
        var product = Product.Create("Test Product", null, Guid.NewGuid(), Guid.NewGuid());
        product.AddAttribute("Material", "Cotton");

        Assert.Throws<InvalidOperationException>(() =>
            product.AddAttribute("material", "Polyester"));
    }

    [Fact]
    public void AddImage_AddsImageToCollection()
    {
        var product = Product.Create("Test Product", null, Guid.NewGuid(), Guid.NewGuid());
        product.AddImage("https://example.com/image.jpg");

        Assert.Single(product.Images);
    }

    [Fact]
    public void ReplaceAttributes_ReplacesTheWholeList()
    {
        var product = Product.Create("Phone", null, Guid.NewGuid(), Guid.NewGuid());
        product.AddAttribute("Battery", "4000mAh");
        product.AddAttribute("Colour", "Black");

        product.ReplaceAttributes(new[] { ("Battery", "5000mAh"), ("Display", "6.5 inches") });

        Assert.Equal(2, product.Attributes.Count);
        Assert.Contains(product.Attributes, a => a.Name == "Battery" && a.Value == "5000mAh");
        Assert.DoesNotContain(product.Attributes, a => a.Name == "Colour");
    }
}