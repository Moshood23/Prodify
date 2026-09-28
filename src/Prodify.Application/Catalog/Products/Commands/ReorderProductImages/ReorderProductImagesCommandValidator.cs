using FluentValidation;

namespace Prodify.Application.Catalog.Products.Commands.ReorderProductImages;

public class ReorderProductImagesCommandValidator : AbstractValidator<ReorderProductImagesCommand>
{
    public ReorderProductImagesCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.ImageIds)
            .NotEmpty()
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Each photo can only appear once.");
    }
}