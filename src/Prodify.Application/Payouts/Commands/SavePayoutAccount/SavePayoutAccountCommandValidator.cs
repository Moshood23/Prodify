using FluentValidation;

namespace Prodify.Application.Payouts.Commands.SavePayoutAccount;

public class SavePayoutAccountCommandValidator : AbstractValidator<SavePayoutAccountCommand>
{
    public SavePayoutAccountCommandValidator()
    {
        RuleFor(x => x.BankName).NotEmpty().WithMessage("Enter your bank's name.").MaximumLength(100);

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Enter your account number.")
            .Matches(@"^\s*\d{10}\s*$").WithMessage("Account number must be 10 digits.");

        RuleFor(x => x.AccountName).NotEmpty().WithMessage("Enter the name on the account.").MaximumLength(100);
    }
}
