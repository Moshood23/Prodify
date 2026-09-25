using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Inventory.Entities;

namespace Prodify.Application.Ordering.Common;

// Moves the stock held for an order through its life:
// placed -> reserved, paid (or pay on delivery) -> confirmed (leaves the shelf),
// cancelled -> released (reserved) or returned (confirmed).
public static class OrderStock
{
    public static async Task<bool> HasActiveReservationsAsync(
        this IApplicationDbContext context, Guid orderId, CancellationToken cancellationToken) =>
        await context.InventoryItems
            .AnyAsync(i => i.Reservations.Any(r => r.OrderId == orderId && r.Status == ReservationStatus.Active), cancellationToken);

    public static async Task ConfirmOrderStockAsync(
        this IApplicationDbContext context, Guid orderId, CancellationToken cancellationToken)
    {
        foreach (var item in await LoadInventoryForOrderAsync(context, orderId, cancellationToken))
        {
            foreach (var reservation in item.Reservations.Where(r => r.OrderId == orderId && r.Status == ReservationStatus.Active).ToList())
                item.ConfirmReservation(reservation.Id);
        }
    }

    public static async Task ReleaseOrderStockAsync(
        this IApplicationDbContext context, Guid orderId, CancellationToken cancellationToken)
    {
        foreach (var item in await LoadInventoryForOrderAsync(context, orderId, cancellationToken))
        {
            foreach (var reservation in item.Reservations.Where(r => r.OrderId == orderId).ToList())
            {
                if (reservation.Status == ReservationStatus.Active)
                    item.ReleaseReservation(reservation.Id);
                else if (reservation.Status == ReservationStatus.Confirmed)
                    item.ReturnConfirmedReservation(reservation.Id, "Order cancelled");
            }
        }
    }

    private static Task<List<InventoryItem>> LoadInventoryForOrderAsync(
        IApplicationDbContext context, Guid orderId, CancellationToken cancellationToken) =>
        context.InventoryItems
            .Include(i => i.Reservations)
            .Where(i => i.Reservations.Any(r => r.OrderId == orderId))
            .ToListAsync(cancellationToken);
}