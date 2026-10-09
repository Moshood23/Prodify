using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Ordering.Common;
using Prodify.Application.Payments.Commands.ProcessPayment;
using Prodify.Domain.Ordering.ValueObjects;
using Prodify.Domain.Payments.Entities;

namespace Prodify.Application.Payments.Commands.ConfirmPaystackPayment;

public class ConfirmPaystackPaymentCommandHandler : IRequestHandler<ConfirmPaystackPaymentCommand, PaystackPaymentOutcome>
{
    // Starts the reason on attempts whose money was sent back, so asking again gives the same answer.
    private const string RefundedNote = "Refunded:";

    private readonly IApplicationDbContext _context;
    private readonly IPaymentService _paymentService;
    private readonly OrderEmailSender _orderEmails;

    public ConfirmPaystackPaymentCommandHandler(IApplicationDbContext context, IPaymentService paymentService, OrderEmailSender orderEmails)
    {
        _context = context;
        _paymentService = paymentService;
        _orderEmails = orderEmails;
    }

    public async Task<PaystackPaymentOutcome> Handle(ConfirmPaystackPaymentCommand request, CancellationToken cancellationToken)
    {
        if (_paymentService.Provider != PaymentProviders.Paystack)
            throw new BusinessRuleException("Paystack is not switched on.");

        var reference = request.Reference.Trim();

        var paymentId = await _context.Payments
            .Where(p => p.Attempts.Any(a => a.GatewayReference == reference))
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (paymentId is null)
            throw new NotFoundException("Payment", reference);

        // The customer's return and the webhook usually arrive together. Locking the payment row
        // makes the second one wait, then see what the first did, so an order is never paid twice.
        await _context.LockPaymentAsync(paymentId.Value, cancellationToken);

        var payment = await _context.Payments
            .Include(p => p.Attempts)
            .FirstAsync(p => p.Id == paymentId, cancellationToken);

        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.SellerOrders)
            .FirstAsync(o => o.Id == payment.OrderId, cancellationToken);

        if (request.CustomerId is not null && order.CustomerId != request.CustomerId)
            throw new NotFoundException("Payment", reference);

        var attempt = payment.Attempts.First(a => a.GatewayReference == reference);

        // Already settled by the other caller.
        if (attempt.Status == PaymentAttemptStatus.Succeeded)
            return Outcome(PaystackPaymentStatuses.Paid);

        if (attempt.Status == PaymentAttemptStatus.Failed)
            return attempt.FailureReason?.StartsWith(RefundedNote, StringComparison.Ordinal) == true
                ? Outcome(PaystackPaymentStatuses.Refunded, attempt.FailureReason[RefundedNote.Length..].Trim())
                : Outcome(PaystackPaymentStatuses.Failed, attempt.FailureReason);

        var transaction = await _paymentService.VerifyAsync(reference, cancellationToken);

        if (transaction.Status == GatewayTransactionStatus.Pending)
            return Outcome(PaystackPaymentStatuses.Pending);

        if (transaction.Status == GatewayTransactionStatus.Failed)
        {
            payment.FailAttempt(attempt.Id, transaction.FailureReason ?? "The payment was declined.");
            return Outcome(PaystackPaymentStatuses.Failed, transaction.FailureReason);
        }

        var problem = await WhyMoneyCannotBeKeptAsync(payment, order, transaction.Amount, cancellationToken);
        if (problem is not null)
        {
            // Paystack took the money but the order can't use it: send it all back.
            var refund = await _paymentService.RefundAsync(reference, Money.Create(transaction.Amount), cancellationToken);
            if (!refund.Succeeded)
                throw new BusinessRuleException($"Your payment arrived after {problem}, and sending it back failed: {refund.FailureReason}. We'll try again.");

            var message = $"Your payment arrived after {problem}, so all {Naira.Format(transaction.Amount)} is being sent back to your card.";
            payment.FailAttempt(attempt.Id, $"{RefundedNote} {message}");
            return Outcome(PaystackPaymentStatuses.Refunded, message);
        }

        payment.CompleteAttempt(attempt.Id, reference);
        order.MarkAsPaid(payment.Id);
        await _context.ConfirmOrderStockAsync(order.Id, cancellationToken);
        await _orderEmails.OrderConfirmedAsync(order, cancellationToken);

        return Outcome(PaystackPaymentStatuses.Paid);
    }

    private async Task<string?> WhyMoneyCannotBeKeptAsync(Payment payment, Domain.Ordering.Entities.Order order, decimal paid, CancellationToken cancellationToken)
    {
        if (payment.IsCaptured || order.IsPaid)
            return "the order had already been paid";

        if (paid != payment.Amount.Amount || paid != order.Total.Amount)
            return $"the amount changed ({Naira.Format(paid)} paid, {Naira.Format(order.Total.Amount)} due)";

        try
        {
            await ProcessPaymentCommandHandler.EnsureOrderCanBePaidAsync(_context, order, cancellationToken);
            return null;
        }
        catch (BusinessRuleException)
        {
            return "the order was cancelled or its 30 minutes to pay ran out";
        }
    }

    private static PaystackPaymentOutcome Outcome(string status, string? message = null) =>
        new() { Status = status, Message = message };
}
