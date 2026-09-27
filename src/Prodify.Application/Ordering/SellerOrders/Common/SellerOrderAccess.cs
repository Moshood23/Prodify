using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.SellerOrders.Common;

public static class SellerOrderAccess
{
    public static async Task<(Order Order, SellerOrder SellerOrder)> GetManagedSellerOrderAsync(
        this IApplicationDbContext context, ICurrentUserService currentUser, Guid sellerOrderId, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .Include(o => o.SellerOrders)
                .ThenInclude(so => so.Items)
            .Include(o => o.SellerOrders)
                .ThenInclude(so => so.StatusHistory)
            .FirstOrDefaultAsync(o => o.SellerOrders.Any(so => so.Id == sellerOrderId), cancellationToken);

        var sellerOrder = order?.SellerOrders.First(so => so.Id == sellerOrderId);

        if (order is null
            || sellerOrder is null
            || !currentUser.CanManageSeller(sellerOrder.SellerId)
            || (!currentUser.IsAdmin() && !SellerOrderRules.IsReadyForSeller(order)))
            throw new NotFoundException("SellerOrder", sellerOrderId);

        return (order, sellerOrder);
    }
}