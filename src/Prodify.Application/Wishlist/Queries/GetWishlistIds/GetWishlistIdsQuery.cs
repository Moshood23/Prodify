using MediatR;

namespace Prodify.Application.Wishlist.Queries.GetWishlistIds;

// The ids of every product the customer saved, so the site can fill in the hearts.
public class GetWishlistIdsQuery : IRequest<List<Guid>>
{
}
