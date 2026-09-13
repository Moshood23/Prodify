using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Inventory.Entities;

namespace Prodify.Application.Inventory.Commands.CreateInventoryItem;

public class CreateInventoryItemCommandHandler : IRequestHandler<CreateInventoryItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateInventoryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var alreadyExists = await _context.InventoryItems
            .AnyAsync(i => i.ProductVariantId == request.ProductVariantId && i.WarehouseId == request.WarehouseId, cancellationToken);

        if (alreadyExists)
            throw new ConflictException("An inventory item already exists for this variant and warehouse.");

        var item = InventoryItem.Create(request.ProductVariantId, request.WarehouseId, request.InitialQuantity);

        _context.Add(item);

        await _context.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}