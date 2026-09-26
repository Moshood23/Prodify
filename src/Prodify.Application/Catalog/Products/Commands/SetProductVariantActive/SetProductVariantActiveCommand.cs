using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.SetProductVariantActive;

// Stops (or resumes) selling one variant, e.g. a size that is discontinued.
public class SetProductVariantActiveCommand : IRequest
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public bool IsActive { get; set; }
}