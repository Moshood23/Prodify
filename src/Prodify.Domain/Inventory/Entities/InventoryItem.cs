using Prodify.Domain.Common;
using Prodify.Domain.Inventory.Events;

namespace Prodify.Domain.Inventory.Entities;

public class InventoryItem : AuditableEntity
{
    private readonly List<StockReservation> _reservations = new();
    private readonly List<StockMovement> _movements = new();

    public Guid ProductVariantId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public int QuantityOnHand { get; private set; }

    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();
    public IReadOnlyCollection<StockMovement> Movements => _movements.AsReadOnly();

    public int QuantityReserved => _reservations
        .Where(r => r.Status == ReservationStatus.Active)
        .Sum(r => r.Quantity);

    public int AvailableQuantity => QuantityOnHand - QuantityReserved;

    private InventoryItem()
    {
    }

    private InventoryItem(Guid id, Guid productVariantId, Guid warehouseId, int quantityOnHand) : base(id)
    {
        ProductVariantId = productVariantId;
        WarehouseId = warehouseId;
        QuantityOnHand = quantityOnHand;
    }

    public static InventoryItem Create(Guid productVariantId, Guid warehouseId, int initialQuantity = 0)
    {
        if (initialQuantity < 0)
            throw new ArgumentException("Initial quantity cannot be negative.", nameof(initialQuantity));

        var item = new InventoryItem(Guid.NewGuid(), productVariantId, warehouseId, initialQuantity);

        if (initialQuantity > 0)
            item._movements.Add(StockMovement.Create(item.Id, StockMovementType.Inbound, initialQuantity, "Initial stock"));

        return item;
    }

    public void AdjustStock(int quantity, string? reason = null)
    {
        if (quantity == 0)
            throw new ArgumentException("Adjustment quantity cannot be zero.", nameof(quantity));

        var newQuantityOnHand = QuantityOnHand + quantity;

        if (newQuantityOnHand < 0)
            throw new InvalidOperationException("Adjustment would result in negative stock.");

        QuantityOnHand = newQuantityOnHand;

        var movementType = quantity > 0 ? StockMovementType.Inbound : StockMovementType.Adjustment;
        _movements.Add(StockMovement.Create(Id, movementType, Math.Abs(quantity), reason));
    }

    public StockReservation Reserve(int quantity, TimeSpan expiryDuration)
    {
        if (quantity <= 0)
            throw new ArgumentException("Reservation quantity must be greater than zero.", nameof(quantity));

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException(
                $"Cannot reserve {quantity} units. Only {AvailableQuantity} available.");

        var reservation = StockReservation.Create(Id, quantity, expiryDuration);
        _reservations.Add(reservation);

        _movements.Add(StockMovement.Create(Id, StockMovementType.Reserved, quantity));
        AddDomainEvent(new StockReservedEvent(Id, reservation.Id, quantity));

        return reservation;
    }

    public void ConfirmReservation(Guid reservationId)
    {
        var reservation = GetActiveReservationOrThrow(reservationId);

        reservation.Confirm();
        QuantityOnHand -= reservation.Quantity;

        _movements.Add(StockMovement.Create(Id, StockMovementType.Outbound, reservation.Quantity));
        AddDomainEvent(new StockConfirmedEvent(Id, reservation.Id, reservation.Quantity));
    }

    public void ReleaseReservation(Guid reservationId)
    {
        var reservation = GetActiveReservationOrThrow(reservationId);

        reservation.Release();

        _movements.Add(StockMovement.Create(Id, StockMovementType.Released, reservation.Quantity));
        AddDomainEvent(new StockReleasedEvent(Id, reservation.Id, reservation.Quantity));
    }

    public void ExpireReservation(Guid reservationId)
    {
        var reservation = GetActiveReservationOrThrow(reservationId);

        reservation.Expire();

        _movements.Add(StockMovement.Create(Id, StockMovementType.Released, reservation.Quantity, "Expired"));
        AddDomainEvent(new StockReleasedEvent(Id, reservation.Id, reservation.Quantity));
    }

    private StockReservation GetActiveReservationOrThrow(Guid reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);

        if (reservation is null)
            throw new InvalidOperationException($"Reservation '{reservationId}' not found.");

        if (reservation.Status != ReservationStatus.Active)
            throw new InvalidOperationException($"Reservation '{reservationId}' is not active.");

        return reservation;
    }
}