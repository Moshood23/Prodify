using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Inventory.Entities;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Catalog.Products.Common;

public static class ProductQueries
{
    // What shoppers are allowed to see: an active product, from an approved seller,
    // with at least one active variant (otherwise there is nothing to buy).
    public static IQueryable<Product> WherePubliclyVisible(this IQueryable<Product> products, IApplicationDbContext context) =>
        products.Where(p =>
            p.IsActive
            && context.Sellers.Any(s => s.Id == p.SellerId && s.Status == SellerStatus.Approved)
            && context.ProductVariants.Any(v => v.ProductId == p.Id && v.IsActive == true));

    // Available stock per variant across all warehouses (on hand minus active reservations).
    // Variants with no inventory records are simply missing from the result (= 0 available).
    public static async Task<Dictionary<Guid, int>> GetAvailableStockAsync(
        this IApplicationDbContext context,
        IReadOnlyCollection<Guid> variantIds,
        CancellationToken cancellationToken)
    {
        if (variantIds.Count == 0)
            return new Dictionary<Guid, int>();

        var rows = await context.InventoryItems
            .Where(i => variantIds.Contains(i.ProductVariantId))
            .Select(i => new
            {
                i.ProductVariantId,
                i.QuantityOnHand,
                Reserved = i.Reservations
                    .Where(r => r.Status == ReservationStatus.Active)
                    .Sum(r => r.Quantity)
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.ProductVariantId)
            .ToDictionary(g => g.Key, g => Math.Max(0, g.Sum(r => r.QuantityOnHand - r.Reserved)));
    }
}