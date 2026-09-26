using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Catalog.Products.DTOs;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;
using Prodify.Application.Common.Security;
using Prodify.Domain.Catalog.ValueObjects;

namespace Prodify.Application.Catalog.Products.Queries.ListManagedProducts;

public class ListManagedProductsQueryHandler : IRequestHandler<ListManagedProductsQuery, PaginatedList<ManagedProductSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ListManagedProductsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ManagedProductSummaryDto>> Handle(ListManagedProductsQuery request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.IsAdmin()
            ? request.SellerId ?? throw new BusinessRuleException("Choose a seller to see their products.")
            : _currentUser.GetRequiredSellerId();

        var products = _context.Products.Where(p => p.SellerId == sellerId);

        // Matches part of the product name, or a variant's exact SKU.
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            var sku = SKU.Create(search);
            products = products.Where(p =>
                p.Name.Contains(search)
                || _context.ProductVariants.Any(v => v.ProductId == p.Id && v.SKU == sku));
        }

        products = (request.Filter?.ToLowerInvariant()) switch
        {
            "active" => products.Where(p => p.IsActive),
            "inactive" => products.Where(p => !p.IsActive),
            _ => products
        };

        var totalCount = await products.CountAsync(cancellationToken);

        var items = await products
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ManagedProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = _context.Categories
                    .Where(c => c.Id == p.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefault() ?? "",
                BrandName = _context.Brands
                    .Where(b => b.Id == p.BrandId)
                    .Select(b => b.Name)
                    .FirstOrDefault(),
                ImageUrl = p.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => i.Url)
                    .FirstOrDefault(),
                IsActive = p.IsActive,
                ActiveVariantCount = _context.ProductVariants
                    .Count(v => v.ProductId == p.Id && v.IsActive == true),
                Price = _context.ProductVariants
                    .Where(v => v.ProductId == p.Id && v.IsActive == true)
                    .Min(v => (decimal?)v.Price),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        await SetStockAsync(items, cancellationToken);

        return new PaginatedList<ManagedProductSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }

    private async Task SetStockAsync(List<ManagedProductSummaryDto> items, CancellationToken cancellationToken)
    {
        var productIds = items.Select(i => i.Id).ToList();

        var variants = await _context.ProductVariants
            .Where(v => productIds.Contains(v.ProductId) && v.IsActive == true)
            .Select(v => new { v.Id, v.ProductId })
            .ToListAsync(cancellationToken);

        var stock = await _context.GetAvailableStockAsync(variants.Select(v => v.Id).ToList(), cancellationToken);

        foreach (var item in items)
        {
            item.AvailableStock = variants
                .Where(v => v.ProductId == item.Id)
                .Sum(v => stock.GetValueOrDefault(v.Id));

            item.Status = ManagedProductStatusRules.For(item.IsActive, item.ActiveVariantCount, item.AvailableStock);
        }
    }
}