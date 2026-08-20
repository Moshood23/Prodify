using Prodify.Domain.Common;

namespace Prodify.Domain.Inventory.Entities;

public enum ReservationStatus
{
    Active,
    Confirmed,
    Released,
    Expired
}

public class StockReservation : Entity
{
    public Guid InventoryItemId { get; private set; }
    public int Quantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime ReservedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private StockReservation()
    {
    }

    internal StockReservation(
        Guid id,
        Guid inventoryItemId,
        int quantity,
        DateTime expiresAt) : base(id)
    {
        InventoryItemId = inventoryItemId;
        Quantity = quantity;
        Status = ReservationStatus.Active;
        ReservedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
    }

    internal static StockReservation Create(Guid inventoryItemId, int quantity, TimeSpan expiryDuration)
    {
        if (quantity <= 0)
            throw new ArgumentException("Reservation quantity must be greater than zero.", nameof(quantity));

        return new StockReservation(Guid.NewGuid(), inventoryItemId, quantity, DateTime.UtcNow.Add(expiryDuration));
    }

    internal void Confirm()
    {
        if (Status != ReservationStatus.Active)
            throw new InvalidOperationException($"Cannot confirm a reservation with status '{Status}'.");

        Status = ReservationStatus.Confirmed;
    }

    internal void Release()
    {
        if (Status != ReservationStatus.Active)
            throw new InvalidOperationException($"Cannot release a reservation with status '{Status}'.");

        Status = ReservationStatus.Released;
    }

    internal void Expire()
    {
        if (Status != ReservationStatus.Active)
            throw new InvalidOperationException($"Cannot expire a reservation with status '{Status}'.");

        Status = ReservationStatus.Expired;
    }

    public bool IsExpired(DateTime asOf) => Status == ReservationStatus.Active && asOf >= ExpiresAt;
}