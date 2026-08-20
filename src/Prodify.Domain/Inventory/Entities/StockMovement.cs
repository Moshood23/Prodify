using Prodify.Domain.Common;

namespace Prodify.Domain.Inventory.Entities;

public enum StockMovementType
{
    Inbound,
    Outbound,
    Adjustment,
    Reserved,
    Released
}

public class StockMovement : Entity
{
    public Guid InventoryItemId { get; private set; }
    public StockMovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public string? Reason { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private StockMovement()
    {
    }

    internal StockMovement(
        Guid id,
        Guid inventoryItemId,
        StockMovementType type,
        int quantity,
        string? reason) : base(id)
    {
        InventoryItemId = inventoryItemId;
        Type = type;
        Quantity = quantity;
        Reason = reason;
        OccurredAt = DateTime.UtcNow;
    }

    internal static StockMovement Create(Guid inventoryItemId, StockMovementType type, int quantity, string? reason = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Movement quantity must be greater than zero.", nameof(quantity));

        return new StockMovement(Guid.NewGuid(), inventoryItemId, type, quantity, reason);
    }
}