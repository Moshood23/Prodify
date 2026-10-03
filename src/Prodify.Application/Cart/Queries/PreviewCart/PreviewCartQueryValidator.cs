using FluentValidation;
using Prodify.Application.Cart.Common;

namespace Prodify.Application.Cart.Queries.PreviewCart;

public class PreviewCartQueryValidator : AbstractValidator<PreviewCartQuery>
{
    public PreviewCartQueryValidator()
    {
        RuleFor(x => x.Items)
            .Must(items => items.Count <= CartRules.MaxGuestCartLines)
            .WithMessage($"A cart can hold at most {CartRules.MaxGuestCartLines} different items.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductVariantId).NotEmpty();
            item.RuleFor(i => i.Quantity).InclusiveBetween(1, CartRules.MaxQuantityPerItem);
        });
    }
}
