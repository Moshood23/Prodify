using FluentValidation;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.SellerOrders.Queries.ListSellerOrders;

public class ListSellerOrdersQueryValidator : AbstractValidator<ListSellerOrdersQuery>
{
    public ListSellerOrdersQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => Enum.TryParse<SellerOrderStatus>(status, ignoreCase: true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<SellerOrderStatus>())}.");

        RuleFor(x => x.Search)
            .MaximumLength(50);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50);
    }
}