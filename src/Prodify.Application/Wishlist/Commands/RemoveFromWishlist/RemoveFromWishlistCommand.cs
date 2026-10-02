using MediatR;

namespace Prodify.Application.Wishlist.Commands.RemoveFromWishlist;

// Removes a product from the customer's wishlist. Removing it again does nothing.
public class RemoveFromWishlistCommand : IRequest
{
    public Guid ProductId { get; set; }
}
