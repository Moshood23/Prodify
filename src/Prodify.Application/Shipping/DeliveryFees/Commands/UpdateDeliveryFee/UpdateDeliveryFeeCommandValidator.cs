using FluentValidation;
using Prodify.Domain.Shipping.Entities;

namespace Prodify.Application.Shipping.DeliveryFees.Commands.UpdateDeliveryFee;

public class UpdateDeliveryFeeCommandValidator : AbstractValidator<UpdateDeliveryFeeCommand>
{
    public UpdateDeliveryFeeCommandValidator()
    {
        RuleFor(x => x.State)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Fee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The fee can't be negative. Use 0 for free delivery.")
            .LessThanOrEqualTo(DeliveryFee.MaxAmount)
            .WithMessage("The fee can be at most 100,000 naira.");
    }
}
