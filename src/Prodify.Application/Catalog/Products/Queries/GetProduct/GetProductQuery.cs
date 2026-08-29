using MediatR;
using Prodify.Application.Catalog.Products.DTOs;

namespace Prodify.Application.Catalog.Products.Queries.GetProduct;

public class GetProductQuery : IRequest<ProductDto>
{
    public Guid Id { get; set; }
}