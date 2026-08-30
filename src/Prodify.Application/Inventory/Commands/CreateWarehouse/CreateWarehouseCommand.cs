using MediatR;

namespace Prodify.Application.Inventory.Commands.CreateWarehouse;

public class CreateWarehouseCommand : IRequest<Guid>
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Address { get; set; }
}