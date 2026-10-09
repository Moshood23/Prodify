using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Ordering.Common;
using Prodify.Domain.Ordering.Entities;
using Prodify.Domain.Payments.Entities;

namespace Prodify.Application.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly OrderEmailSender _orderEmails;

    public ProcessPaymentCommandHandler(IApplicationDbContext context, IPaymentService paymentService, ICurrentUserService currentUser, OrderEmailSender orderEmails)
    {
        _context = context;
        _currentUser = currentUser;
        _paymentService = paymentService;
        _orderEmails = orderEmails;
    }

    public async Task<Guid> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        if (_paymentService.Provider != PaymentProviders.Simulated)
            throw new BusinessRuleException("Card payments go through Paystack.");
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.SellerOrders)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null || order.CustomerId != _currentUser.CustomerId)
            throw new NotFoundException("Order", request.OrderId);

        await EnsureOrderCanBePaidAsync(_context, order, cancellationToken);

        var payment = Payment.Create(order.Id, order.Total);
        _context.Add(payment);

        var attempt = payment.StartAttempt();
        var result = await _paymentService.ChargeAsync(order.Total, request.PaymentMethodToken, cancellationToken);

        if (result.Succeeded)
        {
            payment.CompleteAttempt(attempt.Id, result.GatewayReference!);
            order.MarkAsPaid(payment.Id);
            await _context.ConfirmOrderStockAsync(order.Id, cancellationToken);
            await _orderEmails.OrderConfirmedAsync(order, cancellationToken);
        }
        else
        {
            payment.FailAttempt(attempt.Id, result.FailureReason);
        }

        return payment.Id;
    }

    // Shared with RetryPayment.
    internal static async Task EnsureOrderCanBePaidAsync(IApplicationDbContext context, Order order, CancellationToken cancellationToken)
    {
        if (order.IsPaid)
            throw new BusinessRuleException("This order is already paid.");

        if (order.Status == OrderStatus.Cancelled)
            throw new BusinessRuleException("This order was cancelled.");

        if (order.PaymentMethod == PaymentMethod.PayOnDelivery)
            throw new BusinessRuleException("This order is paid on delivery.");

        // The stock is only held for a limited time while a card order waits for payment.
        if (!await context.HasActiveReservationsAsync(order.Id, cancellationToken))
            throw new BusinessRuleException("This order was not paid in time and its items were released. Please place a new order.");
    }
}
