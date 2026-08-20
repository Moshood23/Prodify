using Prodify.Domain.Catalog.ValueObjects;
using Prodify.Domain.Common;

namespace Prodify.Domain.Catalog.Entities;

public class ProductVariant : AuditableEntity
{
    public Guid ProductId { get; private set; }
    public SKU SKU { get; private set; } = null!;
    public string? Name { get; private set; }
    public decimal Price { get; private set; }
    public decimal? CompareAtPrice { get; private set; }
    public decimal Weight { get; private set; }
    public bool? IsActive { get; private set; }

    private ProductVariant()
    {
    }

    private ProductVariant(
        Guid id,
        Guid productId,
        SKU sku,
        string? name,
        decimal price,
        decimal? compareAtPrice,
        decimal weight) : base(id)
    {
        ProductId = productId;
        SKU = sku;
        Name = name;
        Price = price;
        CompareAtPrice = compareAtPrice;
        Weight = weight;
        IsActive = true;
    }

    public static ProductVariant Create(
        Guid productId,
        string sku,
        decimal price,
        string? name = null,
        decimal? compareAtPrice = null,
        decimal weight = 0)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductVariant must belong to a valid product.", nameof(productId));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        if (compareAtPrice is < 0)
            throw new ArgumentException("Compare-at price cannot be negative.", nameof(compareAtPrice));

        var skuValue = SKU.Create(sku);

        return new ProductVariant(Guid.NewGuid(), productId, skuValue, name, price, compareAtPrice, weight);
    }

    public void UpdatePrice(decimal price, decimal? compareAtPrice = null)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        if (compareAtPrice is < 0)
            throw new ArgumentException("Compare-at price cannot be negative.", nameof(compareAtPrice));

        Price = price;
        CompareAtPrice = compareAtPrice;
    }

    public void UpdateDetails(string? name, decimal weight)
    {
        Name = name;
        Weight = weight;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}