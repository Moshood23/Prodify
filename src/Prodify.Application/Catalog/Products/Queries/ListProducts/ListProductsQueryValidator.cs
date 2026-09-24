using FluentValidation;

namespace Prodify.Application.Catalog.Products.Queries.ListProducts;

public class ListProductsQueryValidator : AbstractValidator<ListProductsQuery>
{
    public static readonly string[] SortOptions = { "newest", "price_asc", "price_desc", "name" };

    public ListProductsQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(100);

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Must(x => x.MinPrice <= x.MaxPrice)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithName("MaxPrice")
            .WithMessage("Maximum price must be greater than or equal to the minimum price.");

        RuleFor(x => x.Sort)
            .Must(sort => SortOptions.Contains(sort!.ToLowerInvariant()))
            .When(x => !string.IsNullOrWhiteSpace(x.Sort))
            .WithMessage($"Sort must be one of: {string.Join(", ", SortOptions)}.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 60);
    }
}