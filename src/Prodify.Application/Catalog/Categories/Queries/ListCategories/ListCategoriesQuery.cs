using MediatR;

namespace Prodify.Application.Catalog.Categories.Queries.ListCategories;

public class ListCategoriesQuery : IRequest<List<CategoryDto>>
{
}