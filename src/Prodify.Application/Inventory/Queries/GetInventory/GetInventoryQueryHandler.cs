using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Inventory.Queries.GetInventory;

public class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, InventoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetInventoryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<InventoryDto> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
    {
        var variantSellerId = await _context.ProductVariants
            .Where(v => v.Id == request.ProductVariantId)
            .Join(_context.Products, v => v.ProductId, p => p.Id, (v, p) => (Guid?)p.SellerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (variantSellerId is null || !_currentUser.CanManageSeller(variantSellerId.Value))
            throw new NotFoundException("InventoryItem", $"{request.ProductVariantId}/{request.WarehouseId}");

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