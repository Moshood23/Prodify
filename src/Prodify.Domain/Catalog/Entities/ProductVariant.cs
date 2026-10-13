using Prodify.Domain.Catalog.ValueObjects;
using Prodify.Domain.Common;

namespace Prodify.Domain.Catalog.Entities;

public class ProductVariant : AuditableEntity
{
    // Sellers are warned when the units ready to sell drop to this number or fewer.
    public const int DefaultLowStockThreshold = 5;
    public const int MaxLowStockThreshold = 10_000;

    public Guid ProductId { get; private set; }
    public SKU SKU { get; private set; } = null!;
    public string? Name { get; private set; }
    public decimal Price { get; private set; }
    public decimal? CompareAtPrice { get; private set; }
    public decimal Weight { get; private set; }
    public bool? IsActive { get; private set; }

    // 0 turns the low stock warning off for this variant.
    public int LowStockThreshold { get; private set; } = DefaultLowStockThreshold;

    // The seller has been told this variant is running low. Cleared once it is restocked,
    // so the next drop warns them again.
    public bool LowStockAlertSent { get; private set; }

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

    public void SetLowStockThreshold(int threshold)
    {
        if (threshold < 0 || threshold > MaxLowStockThreshold)
            throw new ArgumentException($"Choose a number between 0 and {MaxLowStockThreshold}. 0 turns the warning off.", nameof(threshold));

        LowStockThreshold = threshold;
    }

    public bool IsLowStock(int availableQuantity) =>
        IsActive == true && LowStockThreshold > 0 && availableQuantity <= LowStockThreshold;

    public void MarkLowStockAlertSent() => LowStockAlertSent = true;

    public void ClearLowStockAlert() => LowStockAlertSent = false;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
