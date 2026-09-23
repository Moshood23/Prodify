using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }

    // Only used when an admin creates a product on behalf of a seller.
    // Sellers always create products for their own account.
    public Guid? SellerId { get; set; }
}