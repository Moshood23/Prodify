using FluentValidation;
using Prodify.Application.Cart.Common;

namespace Prodify.Application.Cart.Commands.AddToCart;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, CartRules.MaxQuantityPerItem);
    }
}