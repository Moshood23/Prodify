namespace Prodify.Application.Catalog.Products.DTOs;

// One product card in a listing (home page, category page, search results).
public class ProductSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? BrandName { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = null!;

    // Price of the cheapest active variant ("from NGN ...").
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }

    public string? ImageUrl { get; set; }
    public decimal? Rating { get; set; }
    public int ReviewCount { get; set; }
    public Guid DefaultVariantId { get; set; }
    public int VariantCount { get; set; }
    public bool InStock { get; set; }

    // A flash sale on the variant the card shows (the cheapest), while it has units left.
    public ProductCardSaleDto? FlashSale { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductCardSaleDto
{
    public decimal SalePrice { get; set; }
    public decimal WasPrice { get; set; }
    public DateTime EndsAt { get; set; }
}
