using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.DTOs;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Catalog.Products.Queries.ListProducts;

public class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, PaginatedList<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public ListProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsQueryable();

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                IsActive = p.IsActive,
                ImageUrls = p.Images.Select(i => i.Url).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<ProductDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}