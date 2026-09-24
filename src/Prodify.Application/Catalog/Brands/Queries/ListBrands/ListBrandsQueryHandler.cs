using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Brands.Queries.ListBrands;

public class ListBrandsQueryHandler : IRequestHandler<ListBrandsQuery, List<BrandDto>>
{
    private readonly IApplicationDbContext _context;

    public ListBrandsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BrandDto>> Handle(ListBrandsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Brands
            .OrderBy(b => b.Name)
            .Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                LogoUrl = b.LogoUrl
            })
            .ToListAsync(cancellationToken);
    }
}