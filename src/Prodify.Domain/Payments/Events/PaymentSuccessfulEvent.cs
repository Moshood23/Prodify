using Prodify.Domain.Common;

namespace Prodify.Domain.Payments.Events;

public sealed class PaymentSuccessfulEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }

    public PaymentSuccessfulEvent(Guid paymentId, Guid orderId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
    }
}