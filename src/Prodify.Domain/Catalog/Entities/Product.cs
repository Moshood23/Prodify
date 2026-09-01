using Prodify.Domain.Catalog.Events;
using Prodify.Domain.Common;

namespace Prodify.Domain.Catalog.Entities;

public class Product : AuditableEntity
{
    private readonly List<ProductImage> _images = new();
    private readonly List<ProductAttribute> _attributes = new();

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid? BrandId { get; private set; }
    public Guid SellerId { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
    public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();

    private Product()
    {
    }

    private Product(Guid id, string name, string? description, Guid categoryId, Guid? brandId, Guid sellerId) : base(id)
    {
        Name = name;
        Description = description;
        CategoryId = categoryId;
        BrandId = brandId;
        SellerId = sellerId;
        IsActive = true;
    }

    public static Product Create(string name, string? description, Guid categoryId, Guid sellerId, Guid? brandId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));

        if (sellerId == Guid.Empty)
            throw new ArgumentException("Product must belong to a valid seller.", nameof(sellerId));

        var product = new Product(Guid.NewGuid(), name.Trim(), description, categoryId, brandId, sellerId);
        product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Name));

        return product;
    }

    public void Update(string name, string? description, Guid categoryId, Guid? brandId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description;
        CategoryId = categoryId;
        BrandId = brandId;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public ProductImage AddImage(string url, string? altText = null)
    {
        var displayOrder = _images.Count;
        var image = ProductImage.Create(Id, url, altText, displayOrder);
        _images.Add(image);

        return image;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);

        if (image is not null)
            _images.Remove(image);
    }

    public ProductAttribute AddAttribute(string name, string value)
    {
        if (_attributes.Any(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Attribute '{name}' already exists on this product.");

        var attribute = ProductAttribute.Create(Id, name, value);
        _attributes.Add(attribute);

        return attribute;
    }

    public void RemoveAttribute(Guid attributeId)
    {
        var attribute = _attributes.FirstOrDefault(a => a.Id == attributeId);

        if (attribute is not null)
            _attributes.Remove(attribute);
    }
}