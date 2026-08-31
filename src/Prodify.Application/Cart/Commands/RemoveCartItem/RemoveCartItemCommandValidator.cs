using FluentValidation;

namespace Prodify.Application.Cart.Commands.RemoveCartItem;

public class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty();

        RuleFor(x => x.CartItemId)
            .NotEmpty();
    }
}