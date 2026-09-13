using MediatR;

namespace Prodify.Application.Inventory.Commands.CreateInventoryItem;

public class CreateInventoryItemCommand : IRequest<Guid>
{
    public Guid ProductVariantId { get; set; }
    public Guid WarehouseId { get; set; }
    public int InitialQuantity { get; set; }
}