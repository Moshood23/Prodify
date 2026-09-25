using FluentValidation;
using Prodify.Application.Sellers.Common;

namespace Prodify.Application.Sellers.Commands.RejectSeller;

public class RejectSellerCommandValidator : AbstractValidator<RejectSellerCommand>
{
    public RejectSellerCommandValidator()
    {
        RuleFor(x => x.SellerId).NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(SellerRules.ReasonMaxLength);
    }
}