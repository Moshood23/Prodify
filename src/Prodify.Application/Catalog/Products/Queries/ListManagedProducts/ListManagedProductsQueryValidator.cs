using FluentValidation;

namespace Prodify.Application.Catalog.Products.Queries.ListManagedProducts;

public class ListManagedProductsQueryValidator : AbstractValidator<ListManagedProductsQuery>
{
    public static readonly string[] Filters = { "active", "inactive" };

    public ListManagedProductsQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(64);

        RuleFor(x => x.Filter)
            .Must(filter => Filters.Contains(filter!.ToLowerInvariant()))
            .When(x => !string.IsNullOrWhiteSpace(x.Filter))
            .WithMessage($"Filter must be one of: {string.Join(", ", Filters)}.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50);
    }
}