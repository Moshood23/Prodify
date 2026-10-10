using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Ordering.Common;

namespace Prodify.Application.Ordering.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly OrderEmailSender _orderEmails;

    public CancelOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IPaymentService paymentService, OrderEmailSender orderEmails)
    {
        _context = context;
        _currentUser = currentUser;
        _paymentService = paymentService;
        _orderEmails = orderEmails;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.SellerOrders)
                .ThenInclude(so => so.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null || (!_currentUser.IsAdmin() && order.CustomerId != _currentUser.CustomerId))
            throw new NotFoundException("Order", request.OrderId);

        if (!OrderRules.CanBeCancelled(order))
            throw new BusinessRuleException($"This order can no longer be cancelled (status: {order.Status}).");

        // Refund first: if the gateway says no, nothing is cancelled.
        var cancelling = order.SellerOrders.Where(OrderRules.IsStoppable).ToList();
        var refund = await _context.RefundCancelledPartsAsync(_paymentService, order, cancelling, cancellationToken);

        order.Cancel(request.Reason);

        await _context.ReleaseOrderStockAsync(order.Id, cancellationToken);

        await _orderEmails.PartsCancelledAsync(
            order, cancelling, string.IsNullOrWhiteSpace(request.Reason) ? "Cancelled by you" : request.Reason.Trim(), refund, tellSellers: true, cancellationToken);
    }
}
