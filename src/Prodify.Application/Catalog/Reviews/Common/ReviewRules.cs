using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Catalog.Reviews.Common;

public static class ReviewRules
{
    // Only real buyers can review: one of the customer's orders with this
    // product (any variant) must have been delivered.
    public static Task<bool> HasReceivedProductAsync(
        this IApplicationDbContext context, Guid customerId, Guid productId, CancellationToken cancellationToken) =>
        context.SellerOrders
            .Where(so => so.Status == SellerOrderStatus.Delivered
                && context.Orders.Any(o => o.Id == so.OrderId && o.CustomerId == customerId))
            .SelectMany(so => so.Items)
            .AnyAsync(i => context.ProductVariants.Any(v => v.Id == i.ProductVariantId && v.ProductId == productId), cancellationToken);

    // "Tolu Ade" -> "Tolu A."
    public static string ShortName(string firstName, string lastName) =>
        string.IsNullOrWhiteSpace(lastName) ? firstName.Trim() : $"{firstName.Trim()} {char.ToUpperInvariant(lastName.Trim()[0])}.";
}
