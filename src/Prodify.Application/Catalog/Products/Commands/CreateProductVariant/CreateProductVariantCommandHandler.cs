using MediatR;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Inventory.Entities;

namespace Prodify.Application.Catalog.Products.Commands.CreateProductVariant;

public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateProductVariantCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {
        await _context.Products.GetManagedAsync(_currentUser, request.ProductId, cancellationToken);

        await _context.EnsureSkuIsFreeAsync(request.Sku, cancellationToken);

        var warehouseId = await _context.GetDefaultWarehouseIdAsync(cancellationToken);

        var variant = ProductVariant.Create(
            request.ProductId,
            request.Sku,
            request.Price,
            request.Name,
            request.CompareAtPrice,
            request.Weight);

        _context.Add(variant);

        // Every variant gets a stock record, even with 0 units, so stock can be updated later.
        _context.Add(InventoryItem.Create(variant.Id, warehouseId, request.InitialStock));

        return variant.Id;
    }
}