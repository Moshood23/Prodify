using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Cart.Common;

// What the cart and checkout need to know about a variant right now:
// its live price, whether it can still be bought, and how many are in stock.
public record CartVariantInfo(
    Guid VariantId,
    Guid ProductId,
    Guid SellerId,
    string ProductName,
    string? VariantName,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    int AvailableQuantity);

public static class CartVariantLookup
{
    public static async Task<Dictionary<Guid, CartVariantInfo>> GetCartVariantsAsync(
        this IApplicationDbContext context,
        IReadOnlyCollection<Guid> variantIds,
        CancellationToken cancellationToken)
    {
        if (variantIds.Count == 0)
            return new Dictionary<Guid, CartVariantInfo>();

        var rows = await context.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .Join(context.Products, v => v.ProductId, p => p.Id, (v, p) => new
            {
                VariantId = v.Id,
                ProductId = p.Id,
                p.SellerId,
                ProductName = p.Name,
                VariantName = v.Name,
                v.Price,
                ImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                IsAvailable = v.IsActive == true
                    && p.IsActive
                    && context.Sellers.Any(s => s.Id == p.SellerId && s.Status == SellerStatus.Approved)
            })
            .ToListAsync(cancellationToken);

        var stock = await context.GetAvailableStockAsync(variantIds, cancellationToken);

        return rows.ToDictionary(
            r => r.VariantId,
            r => new CartVariantInfo(
                r.VariantId,
                r.ProductId,
                r.SellerId,
                r.ProductName,
                r.VariantName,
                r.Price,
                r.ImageUrl,
                r.IsAvailable,
                stock.GetValueOrDefault(r.VariantId)));
    }
}