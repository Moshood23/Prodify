using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Ordering.Common;
using Prodify.Application.Payments.Commands.ProcessPayment;

namespace Prodify.Application.Payments.Commands.RetryPayment;

public class RetryPaymentCommandHandler : IRequestHandler<RetryPaymentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;

    public RetryPaymentCommandHandler(IApplicationDbContext context, IPaymentService paymentService, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
        _paymentService = paymentService;
    }

    public async Task Handle(RetryPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment is null)
            throw new NotFoundException("Payment", request.PaymentId);

        var order = await _context.Orders
            .Include(o => o.SellerOrders)
            .FirstOrDefaultAsync(o => o.Id == payment.OrderId, cancellationToken);

        if (order is null || order.CustomerId != _currentUser.CustomerId)
            throw new NotFoundException("Payment", request.PaymentId);

        await ProcessPaymentCommandHandler.EnsureOrderCanBePaidAsync(_context, order, cancellationToken);

        var attempt = payment.StartAttempt();
        var result = await _paymentService.ChargeAsync(payment.Amount, request.PaymentMethodToken, cancellationToken);

        if (result.Succeeded)
        {
            payment.CompleteAttempt(attempt.Id, result.GatewayReference!);
            order.MarkAsPaid(payment.Id);
            await _context.ConfirmOrderStockAsync(order.Id, cancellationToken);
        }
        else
        {
            payment.FailAttempt(attempt.Id, result.FailureReason);
        }
    }
}