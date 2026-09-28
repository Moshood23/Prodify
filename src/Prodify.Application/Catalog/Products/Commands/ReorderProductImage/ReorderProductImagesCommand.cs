using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.ReorderProductImages;

// The product's photo ids in their new order; the first becomes the main photo.
public class ReorderProductImagesCommand : IRequest
{
    public Guid ProductId { get; set; }
    public List<Guid> ImageIds { get; set; } = new();
}