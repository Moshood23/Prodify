using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Catalog.Products.DTOs;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Catalog.Products.Queries.ListProducts;

public class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, PaginatedList<ProductSummaryDto>>
{
    private const int MaxSearchTerms = 5;

    private readonly IApplicationDbContext _context;

    public ListProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ProductSummaryDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var products = _context.Products.WherePubliclyVisible(_context);

        if (request.CategoryId.HasValue)
        {
            var categoryId = request.CategoryId.Value;
            products = products.Where(p =>
                p.CategoryId == categoryId
                || _context.Categories.Any(c => c.Id == p.CategoryId && c.ParentCategoryId == categoryId));
        }

        if (request.BrandId.HasValue)
            products = products.Where(p => p.BrandId == request.BrandId.Value);

        if (request.SellerId.HasValue)
            products = products.Where(p => p.SellerId == request.SellerId.Value);

        // Every word must match the product name, its brand or its category ("samsung phone").
        foreach (var term in SplitSearch(request.Search))
        {
            products = products.Where(p =>
                p.Name.Contains(term)
                || _context.Brands.Any(b => b.Id == p.BrandId && b.Name.Contains(term))
                || _context.Categories.Any(c => c.Id == p.CategoryId && c.Name.Contains(term)));
        }

        // The listed price is the cheapest active variant.
        var priced = products.Select(p => new
        {
            Product = p,
            Price = _context.ProductVariants
                .Where(v => v.ProductId == p.Id && v.IsActive == true)
                .Min(v => (decimal?)v.Price) ?? 0
        });

        if (request.MinPrice.HasValue)
            priced = priced.Where(x => x.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            priced = priced.Where(x => x.Price <= request.MaxPrice.Value);

        priced = (request.Sort?.ToLowerInvariant()) switch
        {
            "price_asc" => priced.OrderBy(x => x.Price).ThenBy(x => x.Product.Name),
            "price_desc" => priced.OrderByDescending(x => x.Price).ThenBy(x => x.Product.Name),
            "name" => priced.OrderBy(x => x.Product.Name),
            _ => priced.OrderByDescending(x => x.Product.CreatedAt).ThenBy(x => x.Product.Id)
        };

        var totalCount = await priced.CountAsync(cancellationToken);

        var items = await priced
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ProductSummaryDto
            {
                Id = x.Product.Id,
                Name = x.Product.Name,
                CategoryId = x.Product.CategoryId,
                CategoryName = _context.Categories
                    .Where(c => c.Id == x.Product.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefault() ?? "",
                BrandName = _context.Brands
                    .Where(b => b.Id == x.Product.BrandId)
                    .Select(b => b.Name)
                    .FirstOrDefault(),
                SellerId = x.Product.SellerId,
                SellerName = _context.Sellers
                    .Where(s => s.Id == x.Product.SellerId)
                    .Select(s => s.BusinessName)
                    .FirstOrDefault() ?? "",
                Price = x.Price,
                CompareAtPrice = _context.ProductVariants
                    .Where(v => v.ProductId == x.Product.Id && v.IsActive == true)
                    .OrderBy(v => v.Price)
                    .Select(v => v.CompareAtPrice)
                    .FirstOrDefault(),
                ImageUrl = x.Product.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => i.Url)
                    .FirstOrDefault(),
                DefaultVariantId = _context.ProductVariants
                    .Where(v => v.ProductId == x.Product.Id && v.IsActive == true)
                    .OrderBy(v => v.Price)
                    .Select(v => v.Id)
                    .FirstOrDefault(),
                VariantCount = _context.ProductVariants
                    .Count(v => v.ProductId == x.Product.Id && v.IsActive == true),
                CreatedAt = x.Product.CreatedAt
            })
            .ToListAsync(cancellationToken);

        await SetStockAsync(items, cancellationToken);

        return new PaginatedList<ProductSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }

    // A product is in stock when any of its active variants has stock.
    private async Task SetStockAsync(List<ProductSummaryDto> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return;

        var productIds = items.Select(i => i.Id).ToList();

        var variants = await _context.ProductVariants
            .Where(v => productIds.Contains(v.ProductId) && v.IsActive == true)
            .Select(v => new { v.Id, v.ProductId })
            .ToListAsync(cancellationToken);

        var stock = await _context.GetAvailableStockAsync(variants.Select(v => v.Id).ToList(), cancellationToken);

        var productsInStock = variants
            .Where(v => stock.GetValueOrDefault(v.Id) > 0)
            .Select(v => v.ProductId)
            .ToHashSet();

        foreach (var item in items)
            item.InStock = productsInStock.Contains(item.Id);
    }

    private static IEnumerable<string> SplitSearch(string? search) =>
        string.IsNullOrWhiteSpace(search)
            ? Array.Empty<string>()
            : search
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxSearchTerms);
}