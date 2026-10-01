using FluentValidation;

namespace Prodify.Application.Admin.Reviews.Queries.ListAdminReviews;

public class ListAdminReviewsQueryValidator : AbstractValidator<ListAdminReviewsQuery>
{
    public ListAdminReviewsQueryValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).When(x => x.Rating.HasValue);

        RuleFor(x => x.Status)
            .Must(s => s is "visible" or "hidden")
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be visible or hidden.");

        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
