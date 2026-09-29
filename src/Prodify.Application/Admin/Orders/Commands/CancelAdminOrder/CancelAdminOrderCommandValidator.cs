using FluentValidation;

namespace Prodify.Application.Admin.Orders.Commands.CancelAdminOrder;

public class CancelAdminOrderCommandValidator : AbstractValidator<CancelAdminOrderCommand>
{
    public CancelAdminOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Tell the customer why the order is being cancelled.")
            .MaximumLength(500);
    }
}