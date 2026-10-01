using FluentValidation;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.Application.Catalog.Reviews.Commands.SubmitReview;

public class SubmitReviewCommandValidator : AbstractValidator<SubmitReviewCommand>
{
    public SubmitReviewCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Choose from 1 to 5 stars.");

        RuleFor(x => x.Title)
            .MaximumLength(ProductReview.MaxTitleLength);

        RuleFor(x => x.Comment)
            .MaximumLength(ProductReview.MaxCommentLength);
    }
}
