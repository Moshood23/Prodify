using MediatR;

namespace Prodify.Application.Catalog.Categories.Queries.ListManagedCategories;

// Admin: every category with how much uses it, to know what can be deleted.
public class ListManagedCategoriesQuery : IRequest<List<ManagedCategoryDto>>
{
}

public class ManagedCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int ProductCount { get; set; }
    public int SubCategoryCount { get; set; }
}