using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Inventory.Entities;

namespace Prodify.Application.Inventory.Commands.CreateInventoryItem;

public class CreateInventoryItemCommandHandler : IRequestHandler<CreateInventoryItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateInventoryItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var variantSellerId = await _context.ProductVariants
            .Where(v => v.Id == request.ProductVariantId)
            .Join(_context.Products, v => v.ProductId, p => p.Id, (v, p) => (Guid?)p.SellerId)
            .FirstOrDefaultAsync(cancellationToken);

        // Sellers can only stock their own products.
        if (variantSellerId is null || !_currentUser.CanManageSeller(variantSellerId.Value))
            throw new NotFoundException("ProductVariant", request.ProductVariantId);

        var warehouseExists = await _context.Warehouses
            .AnyAsync(w => w.Id == request.WarehouseId, cancellationToken);

        if (!warehouseExists)
            throw new NotFoundException("Warehouse", request.WarehouseId);

        var alreadyExists = await _context.InventoryItems
            .AnyAsync(i => i.ProductVariantId == request.ProductVariantId && i.WarehouseId == request.WarehouseId, cancellationToken);

        if (alreadyExists)
            throw new ConflictException("An inventory item already exists for this variant and warehouse.");

        var item = InventoryItem.Create(request.ProductVariantId, request.WarehouseId, request.InitialQuantity);

        _context.Add(item);

        return item.Id;
    }
}