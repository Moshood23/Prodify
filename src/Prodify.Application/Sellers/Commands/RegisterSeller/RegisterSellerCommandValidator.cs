using FluentValidation;
using Prodify.Application.Sellers.Common;

namespace Prodify.Application.Sellers.Commands.RegisterSeller;

public class RegisterSellerCommandValidator : AbstractValidator<RegisterSellerCommand>
{
    public RegisterSellerCommandValidator()
    {
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);

        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);

        RuleFor(x => x.Description).MaximumLength(SellerRules.DescriptionMaxLength);

        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(300);
        RuleFor(x => x.AddressLine2).MaximumLength(300);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
    }
}