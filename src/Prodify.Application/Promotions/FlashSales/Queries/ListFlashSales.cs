using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Promotions.FlashSales.Common;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.Application.Promotions.FlashSales.Queries;

public static class FlashSaleFilters
{
    public const int Limit = 100;

    // "pending" (waiting for review and not past its end), "live", "upcoming", "ended", or empty for all.
    public static IQueryable<FlashSale> WithState(this IQueryable<FlashSale> sales, string? state, DateTime now) =>
        state?.Trim().ToLowerInvariant() switch
        {
            null or "" or "all" => sales,
            "pending" => sales.Where(s => s.Status == FlashSaleStatus.Pending && s.EndsAt > now),
            "live" => sales.RunningAt(now),
            "upcoming" => sales.Where(s => s.Status == FlashSaleStatus.Approved && s.StartsAt > now),
            "ended" => sales.Where(s => s.EndsAt <= now || (s.Status != FlashSaleStatus.Pending && s.Status != FlashSaleStatus.Approved)),
            _ => sales.Where(_ => false)
        };
}

// The signed-in seller's flash sales, newest first.
public class ListSellerFlashSalesQuery : IRequest<List<FlashSaleDto>>
{
    public string? State { get; set; }
}

public class ListSellerFlashSalesQueryHandler : IRequestHandler<ListSellerFlashSalesQuery, List<FlashSaleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ListSellerFlashSalesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public Task<List<FlashSaleDto>> Handle(ListSellerFlashSalesQuery request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.GetRequiredSellerId();
        var now = DateTime.UtcNow;
        var sales = _context.FlashSales.Where(s => s.SellerId == sellerId).WithState(request.State, now);
        return _context.ToFlashSaleDtosAsync(sales, FlashSaleFilters.Limit, now, cancellationToken);
    }
}

// Every seller's flash sales, for Prodify's staff.
public class ListAdminFlashSalesQuery : IRequest<List<FlashSaleDto>>
{
    public string? State { get; set; }
}

public class ListAdminFlashSalesQueryHandler : IRequestHandler<ListAdminFlashSalesQuery, List<FlashSaleDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminFlashSalesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<FlashSaleDto>> Handle(ListAdminFlashSalesQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return _context.ToFlashSaleDtosAsync(_context.FlashSales.WithState(request.State, now), FlashSaleFilters.Limit, now, cancellationToken);
    }
}
