using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.RemoveProductImage;

public class RemoveProductImageCommand : IRequest
{
    public Guid ProductId { get; set; }
    public Guid ImageId { get; set; }
}