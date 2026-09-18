using FluentValidation;

namespace Prodify.Application.Shipping.Commands.MarkDelivered;

public class MarkDeliveredCommandValidator : AbstractValidator<MarkDeliveredCommand>
{
    public MarkDeliveredCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
    }
}