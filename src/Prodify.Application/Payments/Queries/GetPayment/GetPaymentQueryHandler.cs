using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Payments.Queries.GetPayment;

public class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, PaymentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPaymentQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaymentDto> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var ownsOrder = await _context.Orders
            .AnyAsync(o => o.Id == request.OrderId && o.CustomerId == _currentUser.CustomerId, cancellationToken);

        if (!ownsOrder && !_currentUser.IsAdmin())
            throw new NotFoundException("Payment", request.OrderId);

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