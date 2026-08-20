using Prodify.Domain.Common;

namespace Prodify.Domain.Catalog.Events;

public sealed class ProductCreatedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string Name { get; }

    public ProductCreatedEvent(Guid productId, string name)
    {
        ProductId = productId;
        Name = name;
    }
}