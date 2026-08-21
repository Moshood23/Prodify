using Prodify.Domain.Common;

namespace Prodify.Domain.Ordering.Events;

public sealed class OrderPlacedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }

    public OrderPlacedEvent(Guid orderId, Guid customerId)
    {
        OrderId = orderId;
        CustomerId = customerId;
    }
}