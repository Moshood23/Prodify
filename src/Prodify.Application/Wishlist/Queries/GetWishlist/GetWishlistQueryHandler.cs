using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.DTOs;
using Prodify.Application.Catalog.Products.Queries.ListProducts;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Wishlist.Queries.GetWishlist;

public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, PaginatedList<ProductSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISender _sender;

    public GetWishlistQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<PaginatedList<ProductSummaryDto>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        var saved = _context.WishlistItems.Where(w => w.CustomerId == customerId);
        var total = await saved.CountAsync(cancellationToken);

        var ids = await saved
            .OrderByDescending(w => w.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => w.ProductId)
            .ToListAsync(cancellationToken);

        if (ids.Count == 0)
            return new PaginatedList<ProductSummaryDto>(new List<ProductSummaryDto>(), total, request.PageNumber, request.PageSize);

        // The shop's product cards (price, stock, rating). Products that are no longer
        // for sale don't come back, so they drop out of the list.
        var products = await _sender.Send(new ListProductsQuery { Ids = ids, PageSize = ids.Count }, cancellationToken);

        var items = products.Items.OrderBy(p => ids.IndexOf(p.Id)).ToList();
        return new PaginatedList<ProductSummaryDto>(items, total, request.PageNumber, request.PageSize);
    }
}
