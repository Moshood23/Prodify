using FluentValidation;

namespace Prodify.Application.Ordering.Queries.ListMyOrders;

public class ListMyOrdersQueryValidator : AbstractValidator<ListMyOrdersQuery>
{
    public ListMyOrdersQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}