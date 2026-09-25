using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Ordering.Common;

namespace Prodify.Application.Ordering.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CancelOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.SellerOrders)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null || (!_currentUser.IsAdmin() && order.CustomerId != _currentUser.CustomerId))
            throw new NotFoundException("Order", request.OrderId);

        if (!OrderRules.CanBeCancelled(order))
            throw new BusinessRuleException(order.IsPaid
                ? "Paid orders can't be cancelled online yet. Please contact support."
                : $"This order can no longer be cancelled (status: {order.Status}).");

        order.Cancel(request.Reason);

        await _context.ReleaseOrderStockAsync(order.Id, cancellationToken);
    }
}