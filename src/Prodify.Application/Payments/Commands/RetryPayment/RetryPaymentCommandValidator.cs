using FluentValidation;

namespace Prodify.Application.Payments.Commands.RetryPayment;

public class RetryPaymentCommandValidator : AbstractValidator<RetryPaymentCommand>
{
    public RetryPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty();

        RuleFor(x => x.PaymentMethodToken)
            .NotEmpty();
    }
}