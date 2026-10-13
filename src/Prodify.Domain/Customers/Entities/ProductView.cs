using Prodify.Domain.Common;

namespace Prodify.Domain.Customers.Entities;

// A product a logged-in customer looked at, for "Recently viewed". One row per product:
// looking at it again moves it back to the front.
public class ProductView : AuditableEntity
{
    public const int MaxPerCustomer = 20;

    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTime ViewedAt { get; private set; }

    private ProductView()
    {
    }

    private ProductView(Guid id, Guid customerId, Guid productId, DateTime viewedAt) : base(id)
    {
        CustomerId = customerId;
        ProductId = productId;
        ViewedAt = viewedAt;
    }

    public static ProductView Create(Guid customerId, Guid productId, DateTime viewedAt)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("A product view needs a customer.", nameof(customerId));

        if (productId == Guid.Empty)
            throw new ArgumentException("A product view needs a product.", nameof(productId));

        return new ProductView(Guid.NewGuid(), customerId, productId, viewedAt);
    }

    // Keeps the latest time, so merging an older list from the browser never pushes a product back.
    public void Seen(DateTime viewedAt)
    {
        if (viewedAt > ViewedAt)
            ViewedAt = viewedAt;
    }
}
