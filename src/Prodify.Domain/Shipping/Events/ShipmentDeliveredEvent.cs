using Prodify.Domain.Common;

namespace Prodify.Domain.Shipping.Events;

public sealed class ShipmentDeliveredEvent : DomainEvent
{
    public Guid ShipmentId { get; }
    public Guid SellerOrderId { get; }

    public ShipmentDeliveredEvent(Guid shipmentId, Guid sellerOrderId)
    {
        ShipmentId = shipmentId;
        SellerOrderId = sellerOrderId;
    }
}