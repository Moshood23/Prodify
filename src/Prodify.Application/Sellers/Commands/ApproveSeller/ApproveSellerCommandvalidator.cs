using FluentValidation;

namespace Prodify.Application.Sellers.Commands.ApproveSeller;

public class ApproveSellerCommandValidator : AbstractValidator<ApproveSellerCommand>
{
    public ApproveSellerCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty();
    }
}