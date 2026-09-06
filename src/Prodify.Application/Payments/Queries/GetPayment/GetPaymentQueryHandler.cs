using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Payments.Queries.GetPayment;

public class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, PaymentDto>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Where(p => p.OrderId == request.OrderId)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                OrderId = p.OrderId,
                Amount = p.Amount.Amount,
                Status = p.Status.ToString(),
                AttemptCount = p.Attempts.Count
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (payment is null)
            throw new NotFoundException("Payment", request.OrderId);

        return payment;
    }
}