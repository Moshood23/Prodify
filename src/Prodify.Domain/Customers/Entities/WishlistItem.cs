using Prodify.Domain.Common;

namespace Prodify.Domain.Customers.Entities;

// A product a customer saved for later ("heart" on a product).
public class WishlistItem : AuditableEntity
{
    public const int MaxItemsPerCustomer = 100;

    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }

    private WishlistItem()
    {
    }

    private WishlistItem(Guid id, Guid customerId, Guid productId) : base(id)
    {
        CustomerId = customerId;
        ProductId = productId;
    }

    public static WishlistItem Create(Guid customerId, Guid productId) =>
        new(Guid.NewGuid(), customerId, productId);
}
