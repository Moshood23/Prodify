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
            .Include(i => i.Reservations)
            .FirstOrDefaultAsync(
                i => i.ProductVariantId == request.ProductVariantId && i.WarehouseId == request.WarehouseId,
                cancellationToken);

        if (item is null)
            throw new NotFoundException("InventoryItem", $"{request.ProductVariantId}/{request.WarehouseId}");

        return new InventoryDto
        {
            Id = item.Id,
            ProductVariantId = item.ProductVariantId,
            WarehouseId = item.WarehouseId,
            QuantityOnHand = item.QuantityOnHand,
            QuantityReserved = item.QuantityReserved,
            AvailableQuantity = item.AvailableQuantity
        };
    }
}