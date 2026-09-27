using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Categories.Queries.ListManagedCategories;

public class ListManagedCategoriesQueryHandler : IRequestHandler<ListManagedCategoriesQuery, List<ManagedCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public ListManagedCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ManagedCategoryDto>> Handle(ListManagedCategoriesQuery request, CancellationToken cancellationToken) =>
        await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new ManagedCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                ProductCount = _context.Products.Count(p => p.CategoryId == c.Id),
                SubCategoryCount = _context.Categories.Count(child => child.ParentCategoryId == c.Id)
            })
            .ToListAsync(cancellationToken);
}