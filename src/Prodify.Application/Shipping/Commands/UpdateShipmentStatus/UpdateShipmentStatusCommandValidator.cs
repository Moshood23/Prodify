using FluentValidation;

namespace Prodify.Application.Shipping.Commands.UpdateShipmentStatus;

public class UpdateShipmentStatusCommandValidator : AbstractValidator<UpdateShipmentStatusCommand>
{
    public UpdateShipmentStatusCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
