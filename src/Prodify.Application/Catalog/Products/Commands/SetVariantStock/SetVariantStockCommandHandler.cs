using MediatR;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Inventory.Entities;

namespace Prodify.Application.Catalog.Products.Commands.SetVariantStock;

public class SetVariantStockCommandHandler : IRequestHandler<SetVariantStockCommand>
{
    private const string Reason = "Stock updated by seller";

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SetVariantStockCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(SetVariantStockCommand request, CancellationToken cancellationToken)
    {
        var variant = await _context.GetManagedVariantAsync(_currentUser, request.ProductId, request.VariantId, cancellationToken);

        var item = await _context.GetInventoryItemAsync(variant.Id, cancellationToken);

        if (item is null)
        {
            var warehouseId = await _context.GetDefaultWarehouseIdAsync(cancellationToken);
            _context.Add(InventoryItem.Create(variant.Id, warehouseId, request.Quantity));
            return;
        }

        item.SetAvailableQuantity(request.Quantity, Reason);
    }
}