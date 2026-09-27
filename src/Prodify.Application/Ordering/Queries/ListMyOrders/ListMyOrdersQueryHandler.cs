using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;
using Prodify.Application.Common.Security;
using Prodify.Application.Ordering.Common;

namespace Prodify.Application.Ordering.Queries.ListMyOrders;

public class ListMyOrdersQueryHandler : IRequestHandler<ListMyOrdersQuery, PaginatedList<OrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ListMyOrdersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<OrderSummaryDto>> Handle(ListMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        var query = _context.Orders.Where(o => o.CustomerId == customerId);
        var totalCount = await query.CountAsync(cancellationToken);

        // Status and total are calculated from the items and seller orders, so load them.
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(o => o.Items)
            .Include(o => o.SellerOrders)
            .ToListAsync(cancellationToken);

        var items = await _context.ToSummariesAsync(orders, cancellationToken);

        return new PaginatedList<OrderSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}