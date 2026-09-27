using MediatR;

namespace Prodify.Application.Catalog.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Empty for a top-level category.
    public Guid? ParentCategoryId { get; set; }
}