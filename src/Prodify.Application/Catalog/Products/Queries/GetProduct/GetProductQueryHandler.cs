using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Catalog.Products.DTOs;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.Application.Catalog.Products.Queries.GetProduct;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetProductQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ProductDetailsDto> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductDetailsDto
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

        if (product is null)
            throw new NotFoundException(nameof(Product), request.Id);

        // Hidden products (inactive, unapproved seller, nothing to buy) are only
        // visible to the seller who owns them and to admins.
        var isPublic = await _context.Products
            .Where(p => p.Id == request.Id)
            .WherePubliclyVisible(_context)
            .AnyAsync(cancellationToken);

        if (!isPublic && !_currentUser.CanManageSeller(product.SellerId))
            throw new NotFoundException(nameof(Product), request.Id);

        var variants = await _context.ProductVariants
            .Where(v => v.ProductId == request.Id && v.IsActive == true)
            .OrderBy(v => v.Price)
            .ToListAsync(cancellationToken);

        var stock = await _context.GetAvailableStockAsync(variants.Select(v => v.Id).ToList(), cancellationToken);

        product.Variants = variants
            .Select(v => new ProductVariantDto
            {
                Id = v.Id,
                Sku = v.SKU.Value,
                Name = v.Name,
                Price = v.Price,
                CompareAtPrice = v.CompareAtPrice,
                AvailableQuantity = stock.GetValueOrDefault(v.Id)
            })
            .ToList();

        return product;
    }
}