using MediatR;

namespace Prodify.Application.Catalog.Categories.Commands.DeleteCategory;

// Only an empty category can be deleted: no products and no sub-categories.
public class DeleteCategoryCommand : IRequest
{
    public Guid Id { get; set; }
}