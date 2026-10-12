using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.Application.Promotions.FlashSales.Common;

// A flash sale running now on one variant, as a shopper sees it.
// WasPrice is the crossed-out price; BoughtByCustomer counts the shopper's earlier orders (0 for guests).
public record FlashSaleOffer(
    Guid FlashSaleId,
    decimal SalePrice,
    decimal WasPrice,
    DateTime EndsAt,
    int Quantity,
    int Sold,
    int MaxPerCustomer,
    int BoughtByCustomer)
{
    public int UnitsLeft => Math.Max(0, Quantity - Sold);

    // How many more this shopper can have at the sale price.
    public int Allowance => Math.Max(0, Math.Min(UnitsLeft, MaxPerCustomer - BoughtByCustomer));

    // The sale price is for the whole cart line or none of it: a line asking for more than
    // the allowance is priced normally, and the cart says how many to keep to get the deal.
    public bool Covers(int quantity) => quantity > 0 && quantity <= Allowance;
}

public static class FlashSaleOffers
{
    // Approved and inside its time window (it may be sold out).
    public static IQueryable<FlashSale> RunningAt(this IQueryable<FlashSale> sales, DateTime now) =>
        sales.Where(s => s.Status == FlashSaleStatus.Approved && s.StartsAt <= now && s.EndsAt > now);

    // The flash sale running now on each of these variants, if any. A sale only counts while its
    // price is still below the variant's price (the seller may have lowered the price since).
    public static async Task<Dictionary<Guid, FlashSaleOffer>> GetFlashSaleOffersAsync(
        this IApplicationDbContext context,
        IReadOnlyCollection<Guid> variantIds,
        Guid? customerId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (variantIds.Count == 0)
            return new Dictionary<Guid, FlashSaleOffer>();

        var sales = await context.FlashSales
            .RunningAt(now)
            .Where(s => variantIds.Contains(s.ProductVariantId))
            .Join(context.ProductVariants, s => s.ProductVariantId, v => v.Id, (s, v) => new { Sale = s, v.Price })
            .Where(x => x.Sale.SalePrice < x.Price)
            .Select(x => new
            {
                x.Sale.Id,
                x.Sale.ProductVariantId,
                x.Sale.SalePrice,
                x.Sale.RegularPrice,
                x.Price,
                x.Sale.EndsAt,
                x.Sale.Quantity,
                x.Sale.SoldQuantity,
                x.Sale.MaxPerCustomer
            })
            .ToListAsync(cancellationToken);

        var bought = new Dictionary<Guid, int>();
        if (customerId is { } id && sales.Count > 0)
        {
            var saleIds = sales.Select(s => s.Id).ToList();
            bought = await context.FlashSaleClaims
                .Where(c => c.CustomerId == id && !c.IsReleased && saleIds.Contains(c.FlashSaleId))
                .GroupBy(c => c.FlashSaleId)
                .Select(g => new { g.Key, Units = g.Sum(c => c.Quantity) })
                .ToDictionaryAsync(g => g.Key, g => g.Units, cancellationToken);
        }

        // Sales can't overlap on a variant, but if they ever did the cheaper one wins.
        return sales
            .GroupBy(s => s.ProductVariantId)
            .Select(g => g.OrderBy(s => s.SalePrice).First())
            .ToDictionary(
                s => s.ProductVariantId,
                s => new FlashSaleOffer(
                    s.Id,
                    s.SalePrice,
                    Math.Min(s.Price, s.RegularPrice),
                    s.EndsAt,
                    s.Quantity,
                    s.SoldQuantity,
                    s.MaxPerCustomer,
                    bought.GetValueOrDefault(s.Id)));
    }

    // Placing an order: takes the flash sale units the order was priced with. Checked again under a lock,
    // because other shoppers may have taken the last units since the cart was priced.
    public static async Task TakeFlashSaleUnitsAsync(
        this IApplicationDbContext context,
        Guid orderId,
        Guid customerId,
        IReadOnlyCollection<(Guid FlashSaleId, string ProductName, int Quantity)> lines,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // Always locked in the same order, so two checkouts can't wait on each other.
        foreach (var line in lines.OrderBy(l => l.FlashSaleId))
        {
            await context.LockFlashSaleAsync(line.FlashSaleId, cancellationToken);

            var sale = await context.FlashSales.FirstAsync(s => s.Id == line.FlashSaleId, cancellationToken);
            var bought = await context.FlashSaleClaims
                .Where(c => c.FlashSaleId == sale.Id && c.CustomerId == customerId && !c.IsReleased)
                .SumAsync(c => c.Quantity, cancellationToken);

            if (!sale.IsRunningAt(now) || line.Quantity > sale.UnitsLeft || bought + line.Quantity > sale.MaxPerCustomer)
                throw new BusinessRuleException($"The flash sale on '{line.ProductName}' has just changed. Check your cart and try again.");

            context.Add(FlashSaleClaim.Create(sale, orderId, customerId, line.Quantity, now));
        }
    }

    // A cancelled order (or one seller's part of it, by variant) gives its flash sale units back.
    public static async Task ReleaseFlashSaleUnitsAsync(
        this IApplicationDbContext context,
        Guid orderId,
        IReadOnlyCollection<Guid>? variantIds,
        CancellationToken cancellationToken)
    {
        var claims = await context.FlashSaleClaims
            .Where(c => c.OrderId == orderId && !c.IsReleased)
            .ToListAsync(cancellationToken);

        foreach (var claim in claims.Where(c => variantIds is null || variantIds.Contains(c.ProductVariantId)))
        {
            await context.LockFlashSaleAsync(claim.FlashSaleId, cancellationToken);
            var sale = await context.FlashSales.FirstAsync(s => s.Id == claim.FlashSaleId, cancellationToken);
            claim.Release(sale);
        }
    }
}
