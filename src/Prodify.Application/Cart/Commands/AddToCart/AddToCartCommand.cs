using MediatR;

namespace Prodify.Application.Cart.Commands.AddToCart;

public class AddToCartCommand : IRequest<Guid>
{
    public Guid? CustomerId { get; set; }
    public string? SessionId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
}