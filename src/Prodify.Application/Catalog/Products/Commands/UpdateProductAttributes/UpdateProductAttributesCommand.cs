using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.UpdateProductAttributes;

// Replaces the product's specifications, shown in the "Specifications" table on the product page.
public class UpdateProductAttributesCommand : IRequest
{
    public Guid ProductId { get; set; }
    public List<ProductAttributeInput> Attributes { get; set; } = new();
}

public class ProductAttributeInput
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}