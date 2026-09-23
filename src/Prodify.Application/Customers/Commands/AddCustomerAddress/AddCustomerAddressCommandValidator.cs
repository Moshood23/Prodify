using FluentValidation;

namespace Prodify.Application.Customers.Commands.AddCustomerAddress;

public class AddCustomerAddressCommandValidator : AbstractValidator<AddCustomerAddressCommand>
{
    public AddCustomerAddressCommandValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(100);

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
    }
}