using Prodify.Domain.Common;

namespace Prodify.Domain.Shipping.Events;

public sealed class ShipmentShippedEvent : DomainEvent
{
    public Guid ShipmentId { get; }
    public Guid SellerOrderId { get; }

    public ShipmentShippedEvent(Guid shipmentId, Guid sellerOrderId)
    {
        ShipmentId = shipmentId;
        SellerOrderId = sellerOrderId;
    }
}