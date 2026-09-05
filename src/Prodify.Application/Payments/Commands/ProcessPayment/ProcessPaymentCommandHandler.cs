using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Payments.Entities;

namespace Prodify.Application.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentService _paymentService;

    public ProcessPaymentCommandHandler(IApplicationDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task<Guid> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
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

        await _context.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}