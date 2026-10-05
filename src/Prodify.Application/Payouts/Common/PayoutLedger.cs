using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Payouts.Entities;

namespace Prodify.Application.Payouts.Common;

// A seller's money, worked out from their earnings and payouts:
// OnHold      = delivered less than 7 days ago
// Available   = past the hold, minus payouts already paid or requested
// InProgress  = requested, waiting for an admin to pay it
public record SellerBalance(decimal OnHold, decimal Available, decimal InProgress, decimal PaidOut, decimal TotalEarned);

public static class PayoutLedger
{
    public static async Task<decimal> GetCommissionRateAsync(this IApplicationDbContext context, CancellationToken cancellationToken) =>
        await context.PlatformSettings.Select(s => (decimal?)s.CommissionRate).FirstOrDefaultAsync(cancellationToken)
        ?? PlatformSettings.DefaultCommissionRate;

    // Called when a part of an order is delivered: the seller has now earned it.
    public static async Task RecordEarningAsync(
        this IApplicationDbContext context, Guid orderId, SellerOrder sellerOrder, CancellationToken cancellationToken)
    {
        if (await context.SellerEarnings.AnyAsync(e => e.SellerOrderId == sellerOrder.Id, cancellationToken))
            return;

        var rate = await context.GetCommissionRateAsync(cancellationToken);
        context.Add(SellerEarning.Create(sellerOrder.SellerId, sellerOrder.Id, orderId, sellerOrder.Total.Amount, rate, DateTime.UtcNow));
    }

    public static async Task<SellerBalance> GetSellerBalanceAsync(
        this IApplicationDbContext context, Guid sellerId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var earnings = context.SellerEarnings.Where(e => e.SellerId == sellerId);
        var payouts = context.SellerPayouts.Where(p => p.SellerId == sellerId);

        var cleared = await earnings.Where(e => e.AvailableAt <= now).SumAsync(e => (decimal?)e.NetAmount, cancellationToken) ?? 0;
        var onHold = await earnings.Where(e => e.AvailableAt > now).SumAsync(e => (decimal?)e.NetAmount, cancellationToken) ?? 0;
        var paidOut = await payouts.Where(p => p.Status == PayoutStatus.Paid).SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;
        var inProgress = await payouts.Where(p => p.Status == PayoutStatus.Requested).SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

        return new SellerBalance(
            OnHold: onHold,
            Available: Math.Max(0, cleared - paidOut - inProgress),
            InProgress: inProgress,
            PaidOut: paidOut,
            TotalEarned: cleared + onHold);
    }
}
