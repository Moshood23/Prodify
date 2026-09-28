using FluentValidation;
using Prodify.Application.Catalog.Products.Common;

namespace Prodify.Application.Catalog.Products.Commands.UploadProductImage;

public class UploadProductImageCommandValidator : AbstractValidator<UploadProductImageCommand>
{
    public UploadProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("Choose a photo to upload.")
            .LessThanOrEqualTo(ProductImageRules.MaxUploadBytes)
            .WithMessage("Photos can be at most 5 MB.");

        RuleFor(x => x.AltText)
            .MaximumLength(250);
    }
}