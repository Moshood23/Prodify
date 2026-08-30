using MediatR;

namespace Prodify.Application.Inventory.Commands.ReserveStock;

public class ReserveStockCommand : IRequest<Guid>
{
    public Guid ProductVariantId { get; set; }
    public Guid WarehouseId { get; set; }
    public int Quantity { get; set; }
    public int ExpiryMinutes { get; set; } = 15;
}