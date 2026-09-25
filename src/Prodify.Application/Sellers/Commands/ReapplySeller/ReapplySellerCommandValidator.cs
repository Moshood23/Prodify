using FluentValidation;
using Prodify.Application.Sellers.Common;

namespace Prodify.Application.Sellers.Commands.ReapplySeller;

public class ReapplySellerCommandValidator : AbstractValidator<ReapplySellerCommand>
{
    public ReapplySellerCommandValidator()
    {
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(SellerRules.DescriptionMaxLength);
    }
}