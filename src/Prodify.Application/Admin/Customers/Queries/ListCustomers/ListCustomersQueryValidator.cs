using FluentValidation;

namespace Prodify.Application.Admin.Customers.Queries.ListCustomers;

public class ListCustomersQueryValidator : AbstractValidator<ListCustomersQuery>
{
    public ListCustomersQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(100);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50);
    }
}