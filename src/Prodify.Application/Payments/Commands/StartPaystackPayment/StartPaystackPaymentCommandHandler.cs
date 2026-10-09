using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Payments.Commands.ProcessPayment;
using Prodify.Domain.Payments.Entities;

namespace Prodify.Application.Payments.Commands.StartPaystackPayment;

public class StartPaystackPaymentCommandHandler : IRequestHandler<StartPaystackPaymentCommand, StartPaystackPaymentResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly IAppUrls _urls;

    public StartPaystackPaymentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IPaymentService paymentService, IAppUrls urls)
    {
        _context = context;
        _currentUser = currentUser;
        _paymentService = paymentService;
        _urls = urls;
    }

    public async Task<StartPaystackPaymentResult> Handle(StartPaystackPaymentCommand request, CancellationToken cancellationToken)
    {
        if (_paymentService.Provider != PaymentProviders.Paystack)
            throw new BusinessRuleException("Paystack is not switched on. Pay with the card form instead.");

        // The order's total comes from its items, and its status from its sellers' parts.
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.SellerOrders)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null || order.CustomerId != _currentUser.CustomerId)
            throw new NotFoundException("Order", request.OrderId);

        await ProcessPaymentCommandHandler.EnsureOrderCanBePaidAsync(_context, order, cancellationToken);

        var email = await _context.Customers
            .Where(c => c.Id == order.CustomerId)
            .Select(c => c.Email)
            .FirstAsync(cancellationToken);

        // Each try gets its own Paystack reference, all on the order's one payment.
        var payment = await _context.Payments
            .Include(p => p.Attempts)
            .Where(p => p.OrderId == order.Id)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment is null || payment.IsCaptured || payment.Amount != order.Total)
        {
            payment = Payment.Create(order.Id, order.Total);
            _context.Add(payment);
        }

        var reference = $"{order.OrderNumber.Value}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        payment.StartAttempt(reference);

        // Paystack adds ?trxref=...&reference=... when it sends the customer back.
        var callbackUrl = _urls.Page($"/orders/{order.Id}");
        var authorizationUrl = await _paymentService.StartHostedCheckoutAsync(
            new HostedCheckoutRequest(reference, payment.Amount, email, callbackUrl, order.Id), cancellationToken);

        return new StartPaystackPaymentResult { AuthorizationUrl = authorizationUrl, Reference = reference };
    }
}
