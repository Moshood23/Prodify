using FluentValidation;

namespace Prodify.Application.Admin.Payouts.Commands.ProcessPayout;

public class ProcessPayoutCommandValidator : AbstractValidator<ProcessPayoutCommand>
{
    public ProcessPayoutCommandValidator()
    {
        RuleFor(x => x.PayoutId).NotEmpty();

        RuleFor(x => x.Reference)
            .NotEmpty().WithMessage("Enter the bank transfer reference.")
            .MaximumLength(100)
            .When(x => x.Paid);

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Say why the payout is rejected. The seller will see this.")
            .MaximumLength(500)
            .When(x => !x.Paid);
    }
}
