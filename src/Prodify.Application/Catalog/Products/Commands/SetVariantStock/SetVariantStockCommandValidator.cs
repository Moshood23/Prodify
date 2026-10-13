using FluentValidation;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.Application.Catalog.Products.Commands.SetVariantStock;

public class SetVariantStockCommandValidator : AbstractValidator<SetVariantStockCommand>
{
    public SetVariantStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.VariantId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .InclusiveBetween(0, VariantRules.MaxStock);

        RuleFor(x => x.LowStockThreshold)
            .InclusiveBetween(0, ProductVariant.MaxLowStockThreshold)
            .WithMessage($"Choose a number between 0 and {ProductVariant.MaxLowStockThreshold}. 0 turns the warning off.");
    }
}
