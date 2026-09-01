using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public Guid SellerId { get; set; }
    public Guid? BrandId { get; set; }
}