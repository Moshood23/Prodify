using FluentValidation;
using Prodify.Application.Catalog.Products.Common;

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
    }
}