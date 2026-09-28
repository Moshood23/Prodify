using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.UploadProductImage;

public class UploadProductImageCommand : IRequest<Guid>
{
    public Guid ProductId { get; set; }
    public Stream Content { get; set; } = null!;
    public long Length { get; set; }
    public string? AltText { get; set; }
}