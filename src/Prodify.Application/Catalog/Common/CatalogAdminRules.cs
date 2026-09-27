using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using ValidationException = Prodify.Application.Common.Exceptions.ValidationException;

namespace Prodify.Application.Catalog.Common;

// Checks shared by the admin's category and brand forms.
public static class CatalogAdminRules
{
    // Categories go two levels deep at most: "Phones & Tablets" > "Phones".
    // Names must be unique among categories with the same parent.
    public static async Task EnsureValidCategoryAsync(
        this IApplicationDbContext context, Guid? categoryId, string name, Guid? parentCategoryId, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();

        if (parentCategoryId.HasValue)
        {
            if (parentCategoryId == categoryId)
                throw Invalid("ParentCategoryId", "A category can't be inside itself.");

            var parent = await context.Categories
                .Where(c => c.Id == parentCategoryId.Value)
                .Select(c => new { c.ParentCategoryId })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw Invalid("ParentCategoryId", "Choose a parent category from the list.");

            if (parent.ParentCategoryId.HasValue)
                throw Invalid("ParentCategoryId", "Choose a top-level category as the parent.");

            if (categoryId.HasValue && await context.Categories.AnyAsync(c => c.ParentCategoryId == categoryId, cancellationToken))
                throw Invalid("ParentCategoryId", "This category has sub-categories, so it must stay at the top level.");
        }

        var duplicate = await context.Categories.AnyAsync(
            c => c.Id != categoryId && c.ParentCategoryId == parentCategoryId && c.Name == trimmed,
            cancellationToken);

        if (duplicate)
            throw Invalid("Name", $"There is already a category called '{trimmed}' here.");
    }

    public static async Task EnsureBrandNameIsFreeAsync(
        this IApplicationDbContext context, Guid? brandId, string name, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();

        if (await context.Brands.AnyAsync(b => b.Id != brandId && b.Name == trimmed, cancellationToken))
            throw Invalid("Name", $"There is already a brand called '{trimmed}'.");
    }

    private static ValidationException Invalid(string field, string message) =>
        new(new[] { new ValidationFailure(field, message) });
}