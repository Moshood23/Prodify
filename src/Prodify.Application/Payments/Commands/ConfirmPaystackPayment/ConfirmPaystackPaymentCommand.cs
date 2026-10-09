using FluentValidation;
using MediatR;

namespace Prodify.Application.Payments.Commands.ConfirmPaystackPayment;

// Asks Paystack how a payment went and updates the order. Sent when the customer comes back
// from Paystack and again when Paystack's webhook arrives; whichever is first does the work.
public class ConfirmPaystackPaymentCommand : IRequest<PaystackPaymentOutcome>
{
    public string Reference { get; set; } = null!;

    // Set when a customer asks, so they only see their own payments. Null for Paystack's webhook.
    public Guid? CustomerId { get; set; }
}

public static class PaystackPaymentStatuses
{
    public const string Paid = "Paid";
    public const string Pending = "Pending";
    public const string Failed = "Failed";
    // Money arrived after the order could no longer take it, so it was sent back.
    public const string Refunded = "Refunded";
}

public class PaystackPaymentOutcome
{
    public string Status { get; set; } = null!;
    public string? Message { get; set; }
}

public class ConfirmPaystackPaymentCommandValidator : AbstractValidator<ConfirmPaystackPaymentCommand>
{
    public ConfirmPaystackPaymentCommandValidator()
    {
        RuleFor(x => x.Reference).NotEmpty().MaximumLength(100);
    }
}
