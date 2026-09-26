using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Catalog.Products.DTOs;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Inventory.Entities;

namespace Prodify.Application.Catalog.Products.Queries.GetManagedProduct;

public class GetManagedProductQueryHandler : IRequestHandler<GetManagedProductQuery, ManagedProductDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetManagedProductQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ManagedProductDto> Handle(GetManagedProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Where(p => p.Id == request.Id)
            .Select(p => new ManagedProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = _context.Categories
                    .Where(c => c.Id == p.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefault() ?? "",
                BrandId = p.BrandId,
                BrandName = _context.Brands
                    .Where(b => b.Id == p.BrandId)
                    .Select(b => b.Name)
                    .FirstOrDefault(),
                SellerId = p.SellerId,
                SellerName = _context.Sellers
                    .Where(s => s.Id == p.SellerId)
                    .Select(s => s.BusinessName)
                    .FirstOrDefault() ?? "",
                CreatedAt = p.CreatedAt,
                Images = p.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        Url = i.Url,
                        AltText = i.AltText,
                        DisplayOrder = i.DisplayOrder
                    })
                    .ToList(),
                Attributes = p.Attributes
                    .OrderBy(a => a.Name)
                    .Select(a => new ProductAttributeDto { Name = a.Name, Value = a.Value })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null || !_currentUser.CanManageSeller(product.SellerId))
            throw new NotFoundException("Product", request.Id);

        var variants = await _context.ProductVariants
            .Where(v => v.ProductId == request.Id)
            .OrderByDescending(v => v.IsActive)
            .ThenBy(v => v.Price)
            .ToListAsync(cancellationToken);

        var variantIds = variants.Select(v => v.Id).ToList();

        var stock = await _context.InventoryItems
            .Where(i => variantIds.Contains(i.ProductVariantId))
            .Select(i => new
            {
                i.ProductVariantId,
                i.QuantityOnHand,
                Reserved = i.Reservations
                    .Where(r => r.Status == ReservationStatus.Active)
                    .Sum(r => r.Quantity)
            })
            .ToListAsync(cancellationToken);

        product.Variants = variants
            .Select(v =>
            {
                var rows = stock.Where(s => s.ProductVariantId == v.Id).ToList();
                var onHand = rows.Sum(s => s.QuantityOnHand);
                var reserved = rows.Sum(s => s.Reserved);

                return new ManagedVariantDto
                {
                    Id = v.Id,
                    Sku = v.SKU.Value,
                    Name = v.Name,
                    Price = v.Price,
                    CompareAtPrice = v.CompareAtPrice,
                    Weight = v.Weight,
                    IsActive = v.IsActive == true,
                    QuantityOnHand = onHand,
                    QuantityReserved = reserved,
                    AvailableQuantity = Math.Max(0, onHand - reserved)
                };
            })
            .ToList();

        var active = product.Variants.Where(v => v.IsActive).ToList();
        product.Status = ManagedProductStatusRules.For(product.IsActive, active.Count, active.Sum(v => v.AvailableQuantity));

        return product;
    }
}