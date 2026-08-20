using Prodify.Domain.Common;

namespace Prodify.Domain.Catalog.Entities;

public class ProductImage : Entity
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = null!;
    public string? AltText { get; private set; }
    public int DisplayOrder { get; private set; }

    private ProductImage()
    {
    }

    internal ProductImage(Guid id, Guid productId, string url, string? altText, int displayOrder) : base(id)
    {
        ProductId = productId;
        Url = url;
        AltText = altText;
        DisplayOrder = displayOrder;
    }

    internal static ProductImage Create(Guid productId, string url, string? altText, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Image URL cannot be empty.", nameof(url));

        return new ProductImage(Guid.NewGuid(), productId, url.Trim(), altText, displayOrder);
    }
}