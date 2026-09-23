using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Shipping.Common;

public static class ShipmentAccess
{
    // Admins, or the seller who owns the seller order, can create and update its shipments.
    public static async Task<bool> CanManageAsync(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        Guid sellerOrderId,
        CancellationToken cancellationToken)
    {
        if (currentUser.IsAdmin())
            return true;

        if (!currentUser.SellerId.HasValue)
            return false;

        return await context.SellerOrders
            .AnyAsync(so => so.Id == sellerOrderId && so.SellerId == currentUser.SellerId, cancellationToken);
    }

    // Anyone who can manage it, plus the customer who placed the order, can view it.
    public static async Task<bool> CanViewAsync(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        Guid sellerOrderId,
        CancellationToken cancellationToken)
    {
        if (await CanManageAsync(context, currentUser, sellerOrderId, cancellationToken))
            return true;

        if (!currentUser.CustomerId.HasValue)
            return false;

        return await context.SellerOrders
            .Where(so => so.Id == sellerOrderId)
            .Join(context.Orders, so => so.OrderId, o => o.Id, (so, o) => o.CustomerId)
            .AnyAsync(customerId => customerId == currentUser.CustomerId, cancellationToken);
    }
}