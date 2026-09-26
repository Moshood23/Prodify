using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.UpdateProductVariant;

// The SKU can't be changed: it identifies the variant on past orders.
public class UpdateProductVariantCommand : IRequest
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal Weight { get; set; }
}