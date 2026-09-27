using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Ordering.Queries.ListMyOrders;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.Common;

public static class OrderSummaries
{
    // One list row per order: status, total, "Kettle and 2 more" and the first item's photo.
    // The orders must be loaded with their Items and SellerOrders.
    public static async Task<List<OrderSummaryDto>> ToSummariesAsync(
        this IApplicationDbContext context, IReadOnlyCollection<Order> orders, CancellationToken cancellationToken)
    {
        var firstVariantIds = orders
            .Select(o => o.Items.FirstOrDefault()?.ProductVariantId)
            .OfType<Guid>()
            .Distinct()
            .ToList();

        var images = await context.ProductVariants
            .Where(v => firstVariantIds.Contains(v.Id))
            .Join(context.Products, v => v.ProductId, p => p.Id, (v, p) => new
            {
                VariantId = v.Id,
                ImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.VariantId, x => x.ImageUrl, cancellationToken);

        return orders.Select(o =>
        {
            var first = o.Items.FirstOrDefault();
            var others = o.Items.Count - 1;

            return new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber.Value,
                CreatedAt = o.CreatedAt,
                Status = o.Status.ToString(),
                IsPaid = o.IsPaid,
                PaymentMethod = o.PaymentMethod.ToString(),
                Total = o.Total.Amount,
                ItemCount = o.Items.Sum(i => i.Quantity),
                Summary = first is null ? "" : others > 0 ? $"{first.ProductName} and {others} more" : first.ProductName,
                ImageUrl = first is null ? null : images.GetValueOrDefault(first.ProductVariantId)
            };
        }).ToList();
    }
}