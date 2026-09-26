using MediatR;
using Prodify.Application.Catalog.Products.DTOs;

namespace Prodify.Application.Catalog.Products.Queries.GetManagedProduct;

// A product as its seller (or an admin) edits it.
public class GetManagedProductQuery : IRequest<ManagedProductDto>
{
    public Guid Id { get; set; }
}