using FluentValidation;

namespace Prodify.Application.Identity.Commands.ChangePassword;

// The detailed password rules (uppercase, number, ...) are checked by ASP.NET Identity.
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("Your new password must be different from your current password.");
    }
}