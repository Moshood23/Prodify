using Prodify.Domain.Common;

namespace Prodify.Domain.Ordering.Events;

public sealed class OrderDeliveredEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid SellerOrderId { get; }

    public OrderDeliveredEvent(Guid orderId, Guid sellerOrderId)
    {
        OrderId = orderId;
        SellerOrderId = sellerOrderId;
    }
}