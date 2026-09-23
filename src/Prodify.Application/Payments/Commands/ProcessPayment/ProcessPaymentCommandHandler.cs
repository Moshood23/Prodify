using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Payments.Entities;

namespace Prodify.Application.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;

    public ProcessPaymentCommandHandler(IApplicationDbContext context, IPaymentService paymentService, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
        _paymentService = paymentService;
    }

    public async Task<Guid> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null || order.CustomerId != _currentUser.CustomerId)
            throw new NotFoundException("Order", request.OrderId);

        if (order.IsPaid)
            throw new BusinessRuleException("Order is already paid.");

        var payment = Payment.Create(order.Id, order.Total);
        _context.Add(payment);

        var attempt = payment.StartAttempt();
        var result = await _paymentService.ChargeAsync(order.Total, request.PaymentMethodToken, cancellationToken);

        if (result.Succeeded)
        {
            payment.CompleteAttempt(attempt.Id, result.GatewayReference!);
            order.MarkAsPaid(payment.Id);
        }
        else
        {
            payment.FailAttempt(attempt.Id, result.FailureReason);
        }

        return payment.Id;
    }
}