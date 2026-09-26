using FluentValidation;
using Prodify.Application.Catalog.Products.Common;

namespace Prodify.Application.Catalog.Products.Commands.UpdateProductVariant;

public class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.VariantId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.CompareAtPrice)
            .GreaterThan(x => x.Price)
            .When(x => x.CompareAtPrice.HasValue)
            .WithMessage(VariantRules.CompareAtPriceMessage);

        RuleFor(x => x.Weight)
            .InclusiveBetween(0, VariantRules.MaxWeightKg);
    }
}