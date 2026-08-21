using Prodify.Domain.Common;

namespace Prodify.Domain.Ordering.Events;

public sealed class OrderPaidEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid PaymentId { get; }

    public OrderPaidEvent(Guid orderId, Guid paymentId)
    {
        OrderId = orderId;
        PaymentId = paymentId;
    }
}