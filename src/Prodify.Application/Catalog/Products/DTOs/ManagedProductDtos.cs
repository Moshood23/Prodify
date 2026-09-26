namespace Prodify.Application.Catalog.Products.DTOs;

// Seller Centre (and admin) view of a product: includes hidden products, inactive
// variants and stock figures that shoppers never see.

public static class ManagedProductStatus
{
    public const string Live = "Live";                 // shoppers can see and buy it
    public const string OutOfStock = "OutOfStock";     // visible, but nothing left to sell
    public const string Incomplete = "Incomplete";     // no active variant yet, so hidden
    public const string Inactive = "Inactive";         // switched off by the seller, so hidden
}

public class ManagedProductSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string? BrandName { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int ActiveVariantCount { get; set; }

    // Cheapest active variant; null while the product has no active variant.
    public decimal? Price { get; set; }

    // Units ready to sell across the active variants.
    public int AvailableStock { get; set; }

    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class ManagedProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductAttributeDto> Attributes { get; set; } = new();
    public List<ManagedVariantDto> Variants { get; set; } = new();
}

public class ManagedVariantDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = null!;
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal Weight { get; set; }
    public bool IsActive { get; set; }

    public int QuantityOnHand { get; set; }

    // Held for orders that are not paid or confirmed yet.
    public int QuantityReserved { get; set; }
    public int AvailableQuantity { get; set; }
}