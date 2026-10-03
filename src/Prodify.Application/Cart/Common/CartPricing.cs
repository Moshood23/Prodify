using Prodify.Application.Cart.Queries.GetCart;

namespace Prodify.Application.Cart.Common;

// A cart line before pricing: what is in the cart and the price when it was added.
public record CartLine(Guid Id, Guid ProductVariantId, int Quantity, decimal SavedPrice);

public static class CartPricing
{
    // Prices the lines at today's prices and works out stock problems.
    // Used for a customer's cart and for a guest's cart.
    public static CartDto Build(Guid? cartId, IEnumerable<CartLine> lines, IReadOnlyDictionary<Guid, CartVariantInfo> variants)
    {
        var items = lines
            .Select(line =>
            {
                variants.TryGetValue(line.ProductVariantId, out var variant);
                var unitPrice = variant?.Price ?? line.SavedPrice;
                var isAvailable = variant?.IsAvailable ?? false;

                return new CartItemDto
                {
                    Id = line.Id,
                    ProductVariantId = line.ProductVariantId,
                    ProductId = variant?.ProductId ?? Guid.Empty,
                    ProductName = variant?.ProductName ?? "Product no longer available",
                    VariantName = variant?.VariantName,
                    ImageUrl = variant?.ImageUrl,
                    Quantity = line.Quantity,
                    UnitPrice = unitPrice,
                    Subtotal = unitPrice * line.Quantity,
                    AvailableQuantity = variant?.AvailableQuantity ?? 0,
                    IsAvailable = isAvailable,
                    InStock = isAvailable && (variant?.AvailableQuantity ?? 0) >= line.Quantity
                };
            })
            .OrderBy(i => i.ProductName)
            .ToList();

        return new CartDto
        {
            Id = cartId,
            Items = items,
            ItemCount = items.Sum(i => i.Quantity),
            // Items that can't be bought don't count towards what the customer will pay.
            Total = items.Where(i => i.IsAvailable).Sum(i => i.Subtotal),
            HasProblems = items.Any(i => !i.InStock)
        };
    }
}
