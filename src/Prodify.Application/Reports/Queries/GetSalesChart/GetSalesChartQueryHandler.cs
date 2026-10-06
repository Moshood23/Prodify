using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Reports.Queries.GetSalesChart;

public class GetSalesChartQueryHandler : IRequestHandler<GetSalesChartQuery, SalesChartDto>
{
    // Nigeria is UTC+1 all year (no daylight saving), so "a day" is counted in Lagos time.
    private static readonly TimeSpan NigeriaOffset = TimeSpan.FromHours(1);

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSalesChartQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SalesChartDto> Handle(GetSalesChartQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow + NigeriaOffset);
        var firstDay = today.AddDays(-(request.Days - 1));
        var fromUtc = firstDay.ToDateTime(TimeOnly.MinValue) - NigeriaOffset;

        // A sale is a part of an order that wasn't cancelled, from an order that is real:
        // paid by card, or pay on delivery. Unpaid card orders may still expire, so they wait.
        var parts = _context.SellerOrders
            .Where(so => so.Status != SellerOrderStatus.Cancelled)
            .Join(_context.Orders, so => so.OrderId, o => o.Id, (so, o) => new { Part = so, Order = o })
            .Where(x => x.Order.CreatedAt >= fromUtc
                && (x.Order.IsPaid || x.Order.PaymentMethod == PaymentMethod.PayOnDelivery));

        if (request.OwnSalesOnly)
        {
            var sellerId = _currentUser.GetRequiredSellerId();
            parts = parts.Where(x => x.Part.SellerId == sellerId);
        }

        var rows = await parts
            .Select(x => new
            {
                x.Order.Id,
                x.Order.CreatedAt,
                Sales = x.Part.Items.Sum(i => i.UnitPrice.Amount * i.Quantity),
            })
            .ToListAsync(cancellationToken);

        var byDay = rows
            .GroupBy(r => DateOnly.FromDateTime(r.CreatedAt + NigeriaOffset))
            .ToDictionary(g => g.Key, g => (Sales: g.Sum(r => r.Sales), Orders: g.Select(r => r.Id).Distinct().Count()));

        var points = Enumerable.Range(0, request.Days)
            .Select(i => firstDay.AddDays(i))
            .Select(day => byDay.TryGetValue(day, out var d)
                ? new SalesDayDto { Date = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Sales = d.Sales, Orders = d.Orders }
                : new SalesDayDto { Date = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) })
            .ToList();

        var result = new SalesChartDto
        {
            Days = request.Days,
            Points = points,
            TotalSales = points.Sum(p => p.Sales),
            TotalOrders = rows.Select(r => r.Id).Distinct().Count(),
        };

        if (!request.OwnSalesOnly)
        {
            result.CommissionEarned = await _context.SellerEarnings
                .Where(e => e.EarnedAt >= fromUtc)
                .SumAsync(e => (decimal?)e.Commission, cancellationToken) ?? 0;
        }

        return result;
    }
}
