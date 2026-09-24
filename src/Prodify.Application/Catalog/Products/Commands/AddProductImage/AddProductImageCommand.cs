using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.AddProductImage;

public class AddProductImageCommand : IRequest<Guid>
{
    public Guid ProductId { get; set; }
    public string Url { get; set; } = null!;
    public string? AltText { get; set; }
}