using MediatR;
using Prodify.Application.Common.Models;
using Prodify.Application.Catalog.Products.DTOs;

namespace Prodify.Application.Catalog.Products.Queries.ListProducts;

public class ListProductsQuery : IRequest<PaginatedList<ProductDto>>
{
    public Guid? CategoryId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}