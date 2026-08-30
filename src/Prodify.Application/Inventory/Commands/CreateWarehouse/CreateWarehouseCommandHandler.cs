using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Inventory.Entities;

namespace Prodify.Application.Inventory.Commands.CreateWarehouse;

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateWarehouseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = Warehouse.Create(request.Name, request.Code, request.Address);

        _context.Add(warehouse);

        await _context.SaveChangesAsync(cancellationToken);

        return warehouse.Id;
    }
}