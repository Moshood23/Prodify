using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Inventory.Entities;
using Prodify.Domain.Notifications.Entities;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Inventory.LowStock;

public static class LowStockRules
{
    // The daily email goes out once a day, from 8am in Lagos (7am UTC).
    public const int DailyEmailHourUtc = 7;

    // Longer lists are cut short in the bell and the email; the products page has the full list.
    public const int MaxListed = 20;

    public const string ProductsLink = "/seller/products?filter=low-stock";

    // Variants on sale whose units ready to sell are at or below the seller's warning level.
    public static IQueryable<ProductVariant> LowStockVariants(this IApplicationDbContext context) =>
        context.ProductVariants.Where(v =>
            v.IsActive == true
            && v.LowStockThreshold > 0
            && context.Products.Any(p => p.Id == v.ProductId && p.IsActive)
            && context.InventoryItems.Where(i => i.ProductVariantId == v.Id).Sum(i => i.QuantityOnHand)
               - context.InventoryItems.Where(i => i.ProductVariantId == v.Id)
                   .SelectMany(i => i.Reservations)
                   .Where(r => r.Status == ReservationStatus.Active)
                   .Sum(r => r.Quantity)
               <= v.LowStockThreshold);

    // The start of the current daily email period.
    public static DateTime DailyEmailCutoff(DateTime now)
    {
        var today = now.Date.AddHours(DailyEmailHourUtc);
        return now >= today ? today : today.AddDays(-1);
    }

    public static string Describe(int available) => available <= 0 ? "Out of stock" : $"{available} left";
}

public record LowStockCheckResult(int SellersNotified, int EmailsSent);

// Run every few minutes by a background job: tells sellers (bell) as soon as a product runs low,
// and sends each seller one email a day listing everything that is low or sold out.
public class CheckLowStockCommand : IRequest<LowStockCheckResult>
{
}

public class CheckLowStockCommandHandler : IRequestHandler<CheckLowStockCommand, LowStockCheckResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailQueue _emails;
    private readonly IAppUrls _urls;

    public CheckLowStockCommandHandler(IApplicationDbContext context, IEmailQueue emails, IAppUrls urls)
    {
        _context = context;
        _emails = emails;
        _urls = urls;
    }

    private record LowItem(Guid VariantId, Guid ProductId, Guid SellerId, string Name, string Sku, bool AlertSent, int Available);

    public async Task<LowStockCheckResult> Handle(CheckLowStockCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var lowIds = _context.LowStockVariants().Select(v => v.Id);

        // Restocked, switched off or no longer on sale: the next drop warns again.
        var restocked = await _context.ProductVariants
            .Where(v => v.LowStockAlertSent && !lowIds.Contains(v.Id))
            .ToListAsync(cancellationToken);
        foreach (var variant in restocked)
            variant.ClearLowStockAlert();

        var items = await LoadLowItemsAsync(cancellationToken);
        if (items.Count == 0)
            return new LowStockCheckResult(0, 0);

        var sellerIds = items.Select(i => i.SellerId).Distinct().ToList();
        var sellers = await _context.Sellers
            .Where(s => sellerIds.Contains(s.Id) && s.Status == SellerStatus.Approved)
            .ToListAsync(cancellationToken);

        var notified = 0;
        var emailed = 0;
        var cutoff = LowStockRules.DailyEmailCutoff(now);

        foreach (var seller in sellers)
        {
            var mine = items.Where(i => i.SellerId == seller.Id).OrderBy(i => i.Available).ThenBy(i => i.Name).ToList();

            var newlyLow = mine.Where(i => !i.AlertSent).ToList();
            if (newlyLow.Count > 0)
            {
                _context.Add(BellFor(seller.Id, newlyLow));
                var ids = newlyLow.Select(i => i.VariantId).ToList();
                var variants = await _context.ProductVariants.Where(v => ids.Contains(v.Id)).ToListAsync(cancellationToken);
                foreach (var variant in variants)
                    variant.MarkLowStockAlertSent();
                notified++;
            }

            if (seller.LowStockEmailSentAt is null || seller.LowStockEmailSentAt < cutoff)
            {
                _emails.Enqueue(seller.Email, seller.BusinessName, LowStockEmail(seller.BusinessName, mine));
                seller.RecordLowStockEmail(now);
                emailed++;
            }
        }

        return new LowStockCheckResult(notified, emailed);
    }

    private async Task<List<LowItem>> LoadLowItemsAsync(CancellationToken cancellationToken)
    {
        var rows = await _context.LowStockVariants()
            .Join(_context.Products, v => v.ProductId, p => p.Id, (v, p) => new
            {
                VariantId = v.Id,
                v.ProductId,
                p.SellerId,
                ProductName = p.Name,
                VariantName = v.Name,
                v.SKU,
                v.LowStockAlertSent
            })
            .ToListAsync(cancellationToken);

        var stock = await StockAsync(rows.Select(r => r.VariantId).ToList(), cancellationToken);

        return rows
            .Select(r => new LowItem(
                r.VariantId,
                r.ProductId,
                r.SellerId,
                r.VariantName is null ? r.ProductName : $"{r.ProductName} ({r.VariantName})",
                r.SKU.Value,
                r.LowStockAlertSent,
                stock.GetValueOrDefault(r.VariantId)))
            .ToList();
    }

    private async Task<Dictionary<Guid, int>> StockAsync(List<Guid> variantIds, CancellationToken cancellationToken)
    {
        var rows = await _context.InventoryItems
            .Where(i => variantIds.Contains(i.ProductVariantId))
            .Select(i => new
            {
                i.ProductVariantId,
                i.QuantityOnHand,
                Reserved = i.Reservations.Where(r => r.Status == ReservationStatus.Active).Sum(r => r.Quantity)
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.ProductVariantId)
            .ToDictionary(g => g.Key, g => Math.Max(0, g.Sum(r => r.QuantityOnHand - r.Reserved)));
    }

    private static Notification BellFor(Guid sellerId, List<LowItem> items)
    {
        if (items.Count == 1)
        {
            var item = items[0];
            return item.Available <= 0
                ? Notification.Create(sellerId, NotificationType.LowStock, "Out of stock",
                    $"{item.Name} has sold out. Add stock so shoppers can keep buying it.", $"/seller/products/{item.ProductId}")
                : Notification.Create(sellerId, NotificationType.LowStock, "Running low",
                    $"Only {item.Available} left of {item.Name}. Add stock before it sells out.", $"/seller/products/{item.ProductId}");
        }

        var names = string.Join(", ", items.Take(3).Select(i => $"{i.Name} ({LowStockRules.Describe(i.Available).ToLowerInvariant()})"));
        var more = items.Count > 3 ? $" and {items.Count - 3} more" : "";
        return Notification.Create(sellerId, NotificationType.LowStock, $"{items.Count} products are running low",
            $"{names}{more}.", LowStockRules.ProductsLink);
    }

    private EmailContent LowStockEmail(string businessName, List<LowItem> items)
    {
        var soldOut = items.Count(i => i.Available <= 0);
        var subject = items.Count == 1
            ? $"Low stock: {items[0].Name}"
            : $"Low stock: {items.Count} products need restocking";

        var summary = soldOut == 0
            ? $"{Plural(items.Count, "product is", "products are")} running low."
            : soldOut == items.Count
                ? $"{Plural(items.Count, "product has", "products have")} sold out."
                : $"{Plural(items.Count, "product is", "products are")} running low, and {soldOut} of them {(soldOut == 1 ? "has" : "have")} sold out.";

        var rows = items.Take(LowStockRules.MaxListed)
            .Select(i => ($"{i.Name} (SKU {i.Sku})", LowStockRules.Describe(i.Available), i.Available <= 0))
            .ToList();

        var paragraphs = new List<string>
        {
            $"Hi {businessName},",
            $"{summary} Add stock so shoppers can keep buying them.",
        };
        if (items.Count > LowStockRules.MaxListed)
            paragraphs.Add($"Showing the first {LowStockRules.MaxListed}. See them all in Seller Centre.");

        return EmailLayout.Build(
            subject,
            "Time to restock",
            paragraphs,
            ("See low stock products", _urls.Page(LowStockRules.ProductsLink)),
            "You get this email at most once a day while products are low. Change the warning level for each product in Seller Centre, or set it to 0 to turn it off.",
            rows);
    }

    private static string Plural(int count, string one, string many) => $"{count} {(count == 1 ? one : many)}";
}
