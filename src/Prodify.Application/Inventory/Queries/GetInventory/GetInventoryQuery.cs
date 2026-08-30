using MediatR;

namespace Prodify.Application.Inventory.Queries.GetInventory;

public class GetInventoryQuery : IRequest<InventoryDto>
{
    public Guid ProductVariantId { get; set; }
    public Guid WarehouseId { get; set; }
}

public class InventoryDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid WarehouseId { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int AvailableQuantity { get; set; }
}