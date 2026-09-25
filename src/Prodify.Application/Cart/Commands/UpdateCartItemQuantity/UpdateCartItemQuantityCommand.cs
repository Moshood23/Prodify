using MediatR;

namespace Prodify.Application.Cart.Commands.UpdateCartItemQuantity;

public class UpdateCartItemQuantityCommand : IRequest
{
    public Guid CartItemId { get; set; }
    public int Quantity { get; set; }
}