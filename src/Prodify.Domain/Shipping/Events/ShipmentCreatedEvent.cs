using Prodify.Domain.Common;

namespace Prodify.Domain.Shipping.Events;

public sealed class ShipmentCreatedEvent : DomainEvent
{
    public Guid ShipmentId { get; }
    public Guid SellerOrderId { get; }

    public ShipmentCreatedEvent(Guid shipmentId, Guid sellerOrderId)
    {
        ShipmentId = shipmentId;
        SellerOrderId = sellerOrderId;
    }
}