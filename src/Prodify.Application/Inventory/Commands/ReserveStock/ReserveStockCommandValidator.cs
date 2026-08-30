using FluentValidation;

namespace Prodify.Application.Inventory.Commands.ReserveStock;

public class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEmpty();

        RuleFor(x => x.WarehouseId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.ExpiryMinutes)
            .GreaterThan(0);
    }
}