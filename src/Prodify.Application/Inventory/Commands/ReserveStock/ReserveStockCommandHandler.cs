using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Inventory.Commands.ReserveStock;

public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public ReserveStockCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var inventoryItem = await _context.InventoryItems
            .FirstOrDefaultAsync(
                i => i.ProductVariantId == request.ProductVariantId && i.WarehouseId == request.WarehouseId,
                cancellationToken);

        if (inventoryItem is null)
            throw new NotFoundException("InventoryItem", $"{request.ProductVariantId}/{request.WarehouseId}");

        var reservation = inventoryItem.Reserve(request.Quantity, TimeSpan.FromMinutes(request.ExpiryMinutes));

        await _context.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}