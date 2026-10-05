using FluentValidation;
using Prodify.Domain.Payouts.Entities;

namespace Prodify.Application.Payouts.Commands.RequestPayout;

public class RequestPayoutCommandValidator : AbstractValidator<RequestPayoutCommand>
{
    public RequestPayoutCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(SellerPayout.MinimumAmount)
            .WithMessage($"The smallest payout is \u20A6{SellerPayout.MinimumAmount:N0}.")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Use at most 2 decimal places.");
    }
}
