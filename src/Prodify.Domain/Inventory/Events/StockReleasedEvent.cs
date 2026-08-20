using Prodify.Domain.Common;

namespace Prodify.Domain.Inventory.Events;

public sealed class StockReleasedEvent : DomainEvent
{
    public Guid InventoryItemId { get; }
    public Guid ReservationId { get; }
    public int Quantity { get; }

    public StockReleasedEvent(Guid inventoryItemId, Guid reservationId, int quantity)
    {
        InventoryItemId = inventoryItemId;
        ReservationId = reservationId;
        Quantity = quantity;
    }
}