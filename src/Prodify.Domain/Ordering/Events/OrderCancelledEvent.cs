using Prodify.Domain.Common;

namespace Prodify.Domain.Ordering.Events;

public sealed class OrderCancelledEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string? Reason { get; }

    public OrderCancelledEvent(Guid orderId, string? reason)
    {
        OrderId = orderId;
        Reason = reason;
    }
}