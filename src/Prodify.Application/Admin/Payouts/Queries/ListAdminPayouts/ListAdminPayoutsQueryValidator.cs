using FluentValidation;
using Prodify.Domain.Payouts.Entities;

namespace Prodify.Application.Admin.Payouts.Queries.ListAdminPayouts;

public class ListAdminPayoutsQueryValidator : AbstractValidator<ListAdminPayoutsQuery>
{
    public ListAdminPayoutsQueryValidator()
    {
        RuleFor(x => x.Status)
            .IsEnumName(typeof(PayoutStatus), caseSensitive: false)
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be Requested, Paid or Rejected.");

        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
