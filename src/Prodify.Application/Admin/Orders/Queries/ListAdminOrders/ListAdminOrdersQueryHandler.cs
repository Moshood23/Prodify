using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;
using Prodify.Application.Ordering.Common;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Admin.Orders.Queries.ListAdminOrders;

public class ListAdminOrdersQueryHandler : IRequestHandler<ListAdminOrdersQuery, PaginatedList<AdminOrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AdminOrderSummaryDto>> Handle(ListAdminOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = _context.Orders.AsQueryable();

        // An order's status is worked out from its sellers' parts, so the stages are too.
        orders = (request.Stage?.ToLowerInvariant()) switch
        {
            "awaiting_payment" => orders.Where(o =>
                o.PaymentMethod == PaymentMethod.Card && !o.IsPaid
                && o.SellerOrders.Any(so => so.Status != SellerOrderStatus.Cancelled)),
            "in_progress" => orders.Where(o =>
                (o.PaymentMethod == PaymentMethod.PayOnDelivery || o.IsPaid)
                && o.SellerOrders.Any(so => so.Status == SellerOrderStatus.Pending || so.Status == SellerOrderStatus.Confirmed
                    || so.Status == SellerOrderStatus.Packed || so.Status == SellerOrderStatus.Shipped)),
            "delivered" => orders.Where(o =>
                o.SellerOrders.Any(so => so.Status == SellerOrderStatus.Delivered)
                && o.SellerOrders.All(so => so.Status == SellerOrderStatus.Delivered || so.Status == SellerOrderStatus.Cancelled)),
            "cancelled" => orders.Where(o => o.SellerOrders.All(so => so.Status == SellerOrderStatus.Cancelled)),
            _ => orders
        };

        if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            var method = Enum.Parse<PaymentMethod>(request.PaymentMethod, ignoreCase: true);
            orders = orders.Where(o => o.PaymentMethod == method);
        }

        if (request.CustomerId.HasValue)
            orders = orders.Where(o => o.CustomerId == request.CustomerId.Value);

        if (request.From.HasValue)
            orders = orders.Where(o => o.CreatedAt >= request.From.Value.Date);

        if (request.To.HasValue)
            orders = orders.Where(o => o.CreatedAt < request.To.Value.Date.AddDays(1));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            orders = orders.Where(o =>
                o.OrderNumber.Value.Contains(search)
                || _context.Customers.Any(c => c.Id == o.CustomerId
                    && (c.Email.Contains(search) || (c.FirstName + " " + c.LastName).Contains(search))));
        }

        var totalCount = await orders.CountAsync(cancellationToken);

        var page = await orders
            .OrderByDescending(o => o.CreatedAt)
            .ThenBy(o => o.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(o => o.Items)
            .Include(o => o.SellerOrders)
            .ToListAsync(cancellationToken);

        var summaries = (await _context.ToSummariesAsync(page, cancellationToken)).ToDictionary(s => s.Id);

        var customerIds = page.Select(o => o.CustomerId).Distinct().ToList();
        var customers = await _context.Customers
            .Where(c => customerIds.Contains(c.Id))
            .Select(c => new { c.Id, Name = c.FirstName + " " + c.LastName, c.Email })
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var items = page.Select(o =>
        {
            var summary = summaries[o.Id];
            var customer = customers.GetValueOrDefault(o.CustomerId);

            return new AdminOrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = summary.OrderNumber,
                CreatedAt = summary.CreatedAt,
                Status = summary.Status,
                IsPaid = summary.IsPaid,
                PaymentMethod = summary.PaymentMethod,
                Total = summary.Total,
                ItemCount = summary.ItemCount,
                Summary = summary.Summary,
                ImageUrl = summary.ImageUrl,
                CustomerId = o.CustomerId,
                CustomerName = customer?.Name ?? "Customer",
                CustomerEmail = customer?.Email ?? "",
                SellerCount = o.SellerOrders.Count
            };
        }).ToList();

        return new PaginatedList<AdminOrderSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}