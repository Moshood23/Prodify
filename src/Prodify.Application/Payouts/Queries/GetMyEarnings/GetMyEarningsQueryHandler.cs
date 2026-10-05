using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Payouts.Common;
using Prodify.Domain.Payouts.Entities;

namespace Prodify.Application.Payouts.Queries.GetMyEarnings;

public class GetMyEarningsQueryHandler : IRequestHandler<GetMyEarningsQuery, EarningsDto>
{
    private const int RecentCount = 20;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyEarningsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<EarningsDto> Handle(GetMyEarningsQuery request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.GetRequiredSellerId();
        var balance = await _context.GetSellerBalanceAsync(sellerId, cancellationToken);
        var now = DateTime.UtcNow;

        var account = await _context.PayoutAccounts
            .Where(a => a.SellerId == sellerId)
            .Select(a => new PayoutAccountDto { BankName = a.BankName, AccountNumber = a.AccountNumber, AccountName = a.AccountName })
            .FirstOrDefaultAsync(cancellationToken);

        var earnings = await _context.SellerEarnings
            .Where(e => e.SellerId == sellerId)
            .Join(_context.Orders, e => e.OrderId, o => o.Id, (e, o) => new EarningDto
            {
                OrderId = e.OrderId,
                SellerOrderId = e.SellerOrderId,
                OrderNumber = o.OrderNumber.Value,
                Sales = e.Sales,
                CommissionRate = e.CommissionRate,
                Commission = e.Commission,
                NetAmount = e.NetAmount,
                EarnedAt = e.EarnedAt,
                AvailableAt = e.AvailableAt,
                IsAvailable = e.AvailableAt <= now,
            })
            .OrderByDescending(e => e.EarnedAt)
            .Take(RecentCount)
            .ToListAsync(cancellationToken);

        var payouts = await _context.SellerPayouts
            .Where(p => p.SellerId == sellerId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(RecentCount)
            .Select(PayoutProjections.ToDto)
            .ToListAsync(cancellationToken);

        return new EarningsDto
        {
            Available = balance.Available,
            OnHold = balance.OnHold,
            InProgress = balance.InProgress,
            PaidOut = balance.PaidOut,
            TotalEarned = balance.TotalEarned,
            CommissionRate = await _context.GetCommissionRateAsync(cancellationToken),
            HoldDays = (int)SellerEarning.HoldPeriod.TotalDays,
            MinimumPayout = SellerPayout.MinimumAmount,
            Account = account,
            HasOpenRequest = payouts.Any(p => p.Status == nameof(PayoutStatus.Requested)),
            RecentEarnings = earnings,
            Payouts = payouts,
        };
    }
}
