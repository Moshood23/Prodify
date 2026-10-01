using FluentValidation;

namespace Prodify.Application.Admin.Reviews.Commands.SetReviewHidden;

public class SetReviewHiddenCommandValidator : AbstractValidator<SetReviewHiddenCommand>
{
    public SetReviewHiddenCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Say why the review is being hidden.")
            .MaximumLength(500)
            .When(x => x.Hidden);
    }
}
