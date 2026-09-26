using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.CreateProductVariant;

public class CreateProductVariantCommand : IRequest<Guid>
{
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public string? Name { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal Weight { get; set; }

    // Units ready to sell, kept in Prodify's warehouse.
    public int InitialStock { get; set; }
}