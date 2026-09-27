using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Brands.Queries.ListManagedBrands;

public class ListManagedBrandsQueryHandler : IRequestHandler<ListManagedBrandsQuery, List<ManagedBrandDto>>
{
    private readonly IApplicationDbContext _context;

    public ListManagedBrandsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ManagedBrandDto>> Handle(ListManagedBrandsQuery request, CancellationToken cancellationToken) =>
        await _context.Brands
            .OrderBy(b => b.Name)
            .Select(b => new ManagedBrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                LogoUrl = b.LogoUrl,
                ProductCount = _context.Products.Count(p => p.BrandId == b.Id)
            })
            .ToListAsync(cancellationToken);
}