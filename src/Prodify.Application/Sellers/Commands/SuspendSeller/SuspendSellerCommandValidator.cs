using FluentValidation;
using Prodify.Application.Sellers.Common;

namespace Prodify.Application.Sellers.Commands.SuspendSeller;

public class SuspendSellerCommandValidator : AbstractValidator<SuspendSellerCommand>
{
    public SuspendSellerCommandValidator()
    {
        RuleFor(x => x.SellerId).NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(SellerRules.ReasonMaxLength);
    }
}