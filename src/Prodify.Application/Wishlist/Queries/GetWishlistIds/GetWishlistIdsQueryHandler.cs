using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Wishlist.Queries.GetWishlistIds;

public class GetWishlistIdsQueryHandler : IRequestHandler<GetWishlistIdsQuery, List<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetWishlistIdsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public Task<List<Guid>> Handle(GetWishlistIdsQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        return _context.WishlistItems
            .Where(w => w.CustomerId == customerId)
            .Select(w => w.ProductId)
            .ToListAsync(cancellationToken);
    }
}
