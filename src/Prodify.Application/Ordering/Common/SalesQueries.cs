using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.Common;

public static class SalesQueries
{
    // What was actually sold in these orders: items from parts that weren't
    // cancelled (money for cancelled parts of paid orders was refunded).
    public static IQueryable<SellerOrderItem> SoldItems(this IApplicationDbContext context, IQueryable<Order> orders) =>
        context.SellerOrders
            .Where(so => so.Status != SellerOrderStatus.Cancelled && orders.Any(o => o.Id == so.OrderId))
            .SelectMany(so => so.Items);
}