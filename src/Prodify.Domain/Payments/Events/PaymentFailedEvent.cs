using Prodify.Domain.Common;

namespace Prodify.Domain.Payments.Events;

public sealed class PaymentFailedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public string? Reason { get; }

    public PaymentFailedEvent(Guid paymentId, Guid orderId, string? reason)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Reason = reason;
    }
}