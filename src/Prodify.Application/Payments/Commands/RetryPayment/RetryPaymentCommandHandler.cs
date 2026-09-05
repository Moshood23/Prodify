using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Payments.Commands.RetryPayment;

public class RetryPaymentCommandHandler : IRequestHandler<RetryPaymentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentService _paymentService;

    public RetryPaymentCommandHandler(IApplicationDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task Handle(RetryPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment is null)
            throw new NotFoundException("Payment", request.PaymentId);

        var attempt = payment.StartAttempt();
        var result = await _paymentService.ChargeAsync(payment.Amount, request.PaymentMethodToken, cancellationToken);

        if (result.Succeeded)
        {
            payment.CompleteAttempt(attempt.Id, result.GatewayReference!);

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId, cancellationToken);

            order?.MarkAsPaid(payment.Id);
        }
        else
        {
            payment.FailAttempt(attempt.Id, result.FailureReason);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}