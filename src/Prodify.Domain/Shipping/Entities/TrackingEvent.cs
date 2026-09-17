using Prodify.Domain.Common;

namespace Prodify.Domain.Shipping.Entities;

public class TrackingEvent : Entity
{
    public Guid ShipmentId { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public string? Description { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private TrackingEvent() { }

    internal TrackingEvent(Guid id, Guid shipmentId, ShipmentStatus status, string? description) : base(id)
    {
        ShipmentId = shipmentId;
        Status = status;
        Description = description;
        OccurredAt = DateTime.UtcNow;
    }

    internal static TrackingEvent Create(Guid shipmentId, ShipmentStatus status, string? description = null)
    {
        return new TrackingEvent(Guid.NewGuid(), shipmentId, status, description);
    }
}