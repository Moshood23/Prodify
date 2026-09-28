using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Ordering.Commands.PlaceOrder;
using Prodify.Application.Ordering.Common;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.Commands.CancelUnpaidOrders;

public class CancelUnpaidOrdersCommandHandler : IRequestHandler<CancelUnpaidOrdersCommand, int>
{
    public const string Reason = "Not paid within 30 minutes";

    // Keeps each run short; anything left over is picked up a minute later.
    private const int BatchSize = 100;

    private readonly IApplicationDbContext _context;

    public CancelUnpaidOrdersCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CancelUnpaidOrdersCommand request, CancellationToken cancellationToken)
    {
        var cutoff = request.Now.AddMinutes(-PlaceOrderCommandHandler.PaymentWindowMinutes);

        var orders = await _context.Orders
            .Include(o => o.SellerOrders)
                .ThenInclude(so => so.StatusHistory)
            .Where(o => o.PaymentMethod == PaymentMethod.Card
                && !o.IsPaid
                && o.CreatedAt <= cutoff
                && o.SellerOrders.Any(so => so.Status != SellerOrderStatus.Cancelled))
            .OrderBy(o => o.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        // An admin may already have moved one on by hand; leave those alone.
        orders = orders.Where(OrderRules.CanBeCancelled).ToList();

        foreach (var order in orders)
        {
            order.Cancel(Reason);
            await _context.ReleaseOrderStockAsync(order.Id, cancellationToken);
        }

        return orders.Count;
    }
}