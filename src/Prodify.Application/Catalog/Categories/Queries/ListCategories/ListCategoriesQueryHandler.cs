using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Categories.Queries.ListCategories;

public class ListCategoriesQueryHandler : IRequestHandler<ListCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public ListCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> Handle(ListCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId
            })
            .ToListAsync(cancellationToken);
    }
}