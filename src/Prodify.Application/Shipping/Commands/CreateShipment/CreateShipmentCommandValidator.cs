using FluentValidation;

namespace Prodify.Application.Shipping.Commands.CreateShipment;

public class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.SellerOrderId).NotEmpty();
    }
}