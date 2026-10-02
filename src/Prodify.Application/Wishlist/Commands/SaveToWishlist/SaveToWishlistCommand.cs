using MediatR;

namespace Prodify.Application.Wishlist.Commands.SaveToWishlist;

// Saves a product to the customer's wishlist. Saving it again does nothing.
public class SaveToWishlistCommand : IRequest
{
    public Guid ProductId { get; set; }
}
