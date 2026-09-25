using FluentValidation;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.Commands.PlaceOrder;

public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.RecipientName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.AddressLine1)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.AddressLine2)
            .MaximumLength(300);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.State)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PostalCode)
            .MaximumLength(20);

        RuleFor(x => x.Country)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.PaymentMethod)
            .Must(method => Enum.TryParse<PaymentMethod>(method, ignoreCase: true, out _))
            .WithMessage("Payment method must be 'Card' or 'PayOnDelivery'.");
    }
}