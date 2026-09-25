using FluentValidation;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Sellers.Queries.ListSellers;

public class ListSellersQueryValidator : AbstractValidator<ListSellersQuery>
{
    public ListSellersQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => Enum.TryParse<SellerStatus>(status, ignoreCase: true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<SellerStatus>())}.");

        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}