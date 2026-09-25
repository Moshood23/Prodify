using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Admin.Queries.GetAdminDashboard;

public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    private const int RecentOrderCount = 5;

    private readonly IApplicationDbContext _context;

    public GetAdminDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardDto> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var sellersByStatus = await _context.Sellers
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        var startOfToday = DateTime.UtcNow.Date;

        // Status and total are calculated from the order's items and seller orders.
        var recentOrders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(RecentOrderCount)
            .Include(o => o.Items)
            .Include(o => o.SellerOrders)
            .ToListAsync(cancellationToken);

        var customerIds = recentOrders.Select(o => o.CustomerId).Distinct().ToList();
        var customerNames = await _context.Customers
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.FirstName + " " + c.LastName, cancellationToken);

        return new AdminDashboardDto
        {
            PendingSellers = sellersByStatus.GetValueOrDefault(SellerStatus.PendingVerification),
            ApprovedSellers = sellersByStatus.GetValueOrDefault(SellerStatus.Approved),
            SuspendedSellers = sellersByStatus.GetValueOrDefault(SellerStatus.Suspended),
            RejectedSellers = sellersByStatus.GetValueOrDefault(SellerStatus.Rejected),
            Customers = await _context.Customers.CountAsync(cancellationToken),
            ActiveProducts = await _context.Products.CountAsync(p => p.IsActive, cancellationToken),
            TotalOrders = await _context.Orders.CountAsync(cancellationToken),
            OrdersToday = await _context.Orders.CountAsync(o => o.CreatedAt >= startOfToday, cancellationToken),
            PaidSales = await _context.Orders
                .Where(o => o.IsPaid)
                .SelectMany(o => o.Items)
                .SumAsync(i => (decimal?)(i.UnitPrice.Amount * i.Quantity), cancellationToken) ?? 0,
            RecentOrders = recentOrders.Select(o => new RecentOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber.Value,
                CreatedAt = o.CreatedAt,
                CustomerName = customerNames.GetValueOrDefault(o.CustomerId) ?? "Customer",
                Status = o.Status.ToString(),
                IsPaid = o.IsPaid,
                PaymentMethod = o.PaymentMethod.ToString(),
                Total = o.Total.Amount
            }).ToList()
        };
    }
}