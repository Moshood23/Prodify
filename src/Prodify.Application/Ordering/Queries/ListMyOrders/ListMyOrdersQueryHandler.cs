using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;
using Prodify.Application.Common.Security;

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

        var firstVariantIds = orders
            .Select(o => o.Items.FirstOrDefault()?.ProductVariantId)
            .OfType<Guid>()
            .Distinct()
            .ToList();

        var images = await _context.ProductVariants
            .Where(v => firstVariantIds.Contains(v.Id))
            .Join(_context.Products, v => v.ProductId, p => p.Id, (v, p) => new
            {
                VariantId = v.Id,
                ImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.VariantId, x => x.ImageUrl, cancellationToken);

        var items = orders.Select(o =>
        {
            var first = o.Items.FirstOrDefault();
            var others = o.Items.Count - 1;

            return new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber.Value,
                CreatedAt = o.CreatedAt,
                Status = o.Status.ToString(),
                IsPaid = o.IsPaid,
                PaymentMethod = o.PaymentMethod.ToString(),
                Total = o.Total.Amount,
                ItemCount = o.Items.Sum(i => i.Quantity),
                Summary = first is null ? "" : others > 0 ? $"{first.ProductName} and {others} more" : first.ProductName,
                ImageUrl = first is null ? null : images.GetValueOrDefault(first.ProductVariantId)
            };
        }).ToList();

        return new PaginatedList<OrderSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}