namespace Prodify.Application.Catalog.Products.DTOs;

public class ProductDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = null!;

    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<ProductAttributeDto> Attributes { get; set; } = new();
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = null!;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
}

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = null!;
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int AvailableQuantity { get; set; }
    public bool InStock => AvailableQuantity > 0;
}

public class ProductAttributeDto
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}