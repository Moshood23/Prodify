using MediatR;

namespace Prodify.Application.Catalog.Brands.Commands.DeleteBrand;

// Only a brand that no product uses can be deleted.
public class DeleteBrandCommand : IRequest
{
    public Guid Id { get; set; }
}