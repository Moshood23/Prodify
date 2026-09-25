using FluentValidation;
using Prodify.Application.Cart.Common;

namespace Prodify.Application.Cart.Commands.UpdateCartItemQuantity;

public class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.CartItemId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, CartRules.MaxQuantityPerItem);
    }
}