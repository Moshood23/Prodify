using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Inventory.Queries.GetInventory;

public class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, InventoryDto>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryDto> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .Where(i => i.ProductVariantId == request.ProductVariantId && i.WarehouseId == request.WarehouseId)
            .Select(i => new InventoryDto
            {
                Id = i.Id,
                ProductVariantId = i.ProductVariantId,
                WarehouseId = i.WarehouseId,
                QuantityOnHand = i.QuantityOnHand,
                QuantityReserved = i.QuantityReserved,
                AvailableQuantity = i.AvailableQuantity
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            throw new NotFoundException("InventoryItem", $"{request.ProductVariantId}/{request.WarehouseId}");

        return item;
    }
}