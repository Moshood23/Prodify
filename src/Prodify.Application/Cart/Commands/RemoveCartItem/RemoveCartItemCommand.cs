using MediatR;

namespace Prodify.Application.Cart.Commands.RemoveCartItem;

public class RemoveCartItemCommand : IRequest
{
    public Guid CartItemId { get; set; }
}