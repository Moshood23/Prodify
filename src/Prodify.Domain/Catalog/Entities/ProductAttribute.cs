using Prodify.Domain.Common;

namespace Prodify.Domain.Catalog.Entities;

public class ProductAttribute : Entity
{
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Value { get; private set; } = null!;

    private ProductAttribute()
    {
    }

    internal ProductAttribute(Guid id, Guid productId, string name, string value) : base(id)
    {
        ProductId = productId;
        Name = name;
        Value = value;
    }

    internal static ProductAttribute Create(Guid productId, string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Attribute value cannot be empty.", nameof(value));

        return new ProductAttribute(Guid.NewGuid(), productId, name.Trim(), value.Trim());
    }
}