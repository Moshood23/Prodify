using FluentValidation;

namespace Prodify.Application.Admin.Settings.Commands.UpdatePlatformSettings;

public class UpdatePlatformSettingsCommandValidator : AbstractValidator<UpdatePlatformSettingsCommand>
{
    public UpdatePlatformSettingsCommandValidator()
    {
        RuleFor(x => x.CommissionRate)
            .InclusiveBetween(0, 50).WithMessage("Commission must be between 0% and 50%.")
            .PrecisionScale(5, 2, ignoreTrailingZeros: true).WithMessage("Use at most 2 decimal places.");
    }
}
