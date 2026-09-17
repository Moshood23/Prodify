using Prodify.Domain.Common;
using Prodify.Domain.Shipping.Events;

namespace Prodify.Domain.Shipping.Entities;

public enum ShipmentStatus
{
    Pending,
    Shipped,
    InTransit,
    Delivered,
    Failed
}

public class Shipment : AuditableEntity
{
    private readonly List<ShipmentItem> _items = new();
    private readonly List<TrackingEvent> _trackingEvents = new();

    public Guid SellerOrderId { get; private set; }
    public string? Carrier { get; private set; }
    public string? TrackingNumber { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    public ICollection<ShipmentItem> Items => _items;
    public ICollection<TrackingEvent> TrackingEvents => _trackingEvents;

    private Shipment() { }

    private Shipment(Guid id, Guid sellerOrderId) : base(id)
    {
        SellerOrderId = sellerOrderId;
        Status = ShipmentStatus.Pending;
    }

    public static Shipment Create(Guid sellerOrderId, IEnumerable<(Guid ProductVariantId, int Quantity)> items)
    {
        if (sellerOrderId == Guid.Empty)
            throw new ArgumentException("Shipment must belong to a valid seller order.", nameof(sellerOrderId));

        var shipment = new Shipment(Guid.NewGuid(), sellerOrderId);

        foreach (var item in items)
        {
            shipment._items.Add(ShipmentItem.Create(shipment.Id, item.ProductVariantId, item.Quantity));
        }

        if (!shipment._items.Any())
            throw new ArgumentException("Shipment must contain at least one item.", nameof(items));

        shipment._trackingEvents.Add(TrackingEvent.Create(shipment.Id, ShipmentStatus.Pending, "Shipment created"));
        shipment.AddDomainEvent(new ShipmentCreatedEvent(shipment.Id, sellerOrderId));

        return shipment;
    }

    public void Ship(string carrier, string trackingNumber)
    {
        if (Status != ShipmentStatus.Pending)
            throw new InvalidOperationException($"Cannot ship a shipment with status '{Status}'.");

        if (string.IsNullOrWhiteSpace(carrier))
            throw new ArgumentException("Carrier cannot be empty.", nameof(carrier));

        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number cannot be empty.", nameof(trackingNumber));

        Carrier = carrier;
        TrackingNumber = trackingNumber;
        Status = ShipmentStatus.Shipped;
        ShippedAt = DateTime.UtcNow;

        _trackingEvents.Add(TrackingEvent.Create(Id, ShipmentStatus.Shipped, $"Dispatched via {carrier}"));
        AddDomainEvent(new ShipmentShippedEvent(Id, SellerOrderId));
    }

    public void UpdateStatus(ShipmentStatus status, string? description = null)
    {
        if (Status is ShipmentStatus.Delivered or ShipmentStatus.Failed)
            throw new InvalidOperationException($"Cannot update a shipment with status '{Status}'.");

        Status = status;
        _trackingEvents.Add(TrackingEvent.Create(Id, status, description));

        if (status == ShipmentStatus.Delivered)
        {
            DeliveredAt = DateTime.UtcNow;
            AddDomainEvent(new ShipmentDeliveredEvent(Id, SellerOrderId));
        }
    }
}