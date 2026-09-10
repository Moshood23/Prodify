using FluentValidation;

namespace Prodify.Application.Sellers.Commands.RegisterSeller;

public class RegisterSellerCommandValidator : AbstractValidator<RegisterSellerCommand>
{
    public RegisterSellerCommandValidator()
    {
        RuleFor(x => x.BusinessName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20);
    }
}