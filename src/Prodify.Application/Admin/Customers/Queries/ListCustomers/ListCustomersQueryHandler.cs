using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Admin.Customers.Queries.ListCustomers;

public class ListCustomersQueryHandler : IRequestHandler<ListCustomersQuery, PaginatedList<CustomerSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public ListCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<CustomerSummaryDto>> Handle(ListCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            customers = customers.Where(c =>
                c.Email.Contains(search)
                || (c.FirstName + " " + c.LastName).Contains(search)
                || (c.PhoneNumber != null && c.PhoneNumber.Contains(search)));
        }

        var totalCount = await customers.CountAsync(cancellationToken);

        var items = await customers
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerSummaryDto
            {
                Id = c.Id,
                Name = c.FirstName + " " + c.LastName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                CreatedAt = c.CreatedAt,
                OrderCount = _context.Orders.Count(o => o.CustomerId == c.Id),
                TotalSpent = _context.SellerOrders
                    .Where(so => so.Status != SellerOrderStatus.Cancelled
                        && _context.Orders.Any(o => o.Id == so.OrderId && o.CustomerId == c.Id && o.IsPaid))
                    .SelectMany(so => so.Items)
                    .Sum(i => (decimal?)(i.UnitPrice.Amount * i.Quantity)) ?? 0
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<CustomerSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}