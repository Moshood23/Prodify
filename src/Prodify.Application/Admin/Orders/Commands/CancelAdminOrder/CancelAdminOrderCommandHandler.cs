using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Ordering.Common;

namespace Prodify.Application.Admin.Orders.Commands.CancelAdminOrder;

public class CancelAdminOrderCommandHandler : IRequestHandler<CancelAdminOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentService _paymentService;

    public CancelAdminOrderCommandHandler(IApplicationDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task Handle(CancelAdminOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.SellerOrders)
                .ThenInclude(so => so.Items)
            .Include(o => o.SellerOrders)
                .ThenInclude(so => so.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        if (!OrderRules.CanAdminCancel(order))
            throw new BusinessRuleException(
                $"This order can't be cancelled (status: {order.Status}). Parts that were shipped or delivered can't be stopped.");

        var cancelling = order.SellerOrders.Where(OrderRules.IsStoppable).ToList();
        // Refund first: if the gateway says no, nothing is cancelled.
        var refund = OrderRefunds.RefundFor(order, cancelling);
        await _context.RefundIfPaidAsync(_paymentService, order, refund, cancellationToken);

        order.Cancel(request.Reason.Trim());
        await _context.ReleaseOrderStockAsync(order.Id, cancellationToken);
    }
}