using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Catalog.Entities;
using Prodify.Domain.Catalog.ValueObjects;
using Prodify.Domain.Inventory.Entities;
using ValidationException = Prodify.Application.Common.Exceptions.ValidationException;

namespace Prodify.Application.Catalog.Products.Common;

// Shared steps for the Seller Centre: finding a product the current user may manage,
// checking category/brand, and finding where the stock is kept.
public static class ProductManagement
{
    // A seller's own product (any product for admins). Someone else's product is "not found".
    public static async Task<Product> GetManagedAsync(
        this IQueryable<Product> products, ICurrentUserService currentUser, Guid productId, CancellationToken cancellationToken)
    {
        var product = await products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product is null || !currentUser.CanManageSeller(product.SellerId))
            throw new NotFoundException(nameof(Product), productId);

        return product;
    }

    public static async Task<ProductVariant> GetManagedVariantAsync(
        this IApplicationDbContext context, ICurrentUserService currentUser, Guid productId, Guid variantId, CancellationToken cancellationToken)
    {
        await context.Products.GetManagedAsync(currentUser, productId, cancellationToken);

        return await context.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductVariant), variantId);
    }

    // Reported as form errors, so the seller sees them next to the right field.
    public static async Task EnsureCategoryAndBrandExistAsync(
        this IApplicationDbContext context, Guid categoryId, Guid? brandId, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();

        if (!await context.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken))
            failures.Add(new ValidationFailure("CategoryId", "Choose a category from the list."));

        if (brandId.HasValue && !await context.Brands.AnyAsync(b => b.Id == brandId.Value, cancellationToken))
            failures.Add(new ValidationFailure("BrandId", "Choose a brand from the list."));

        if (failures.Count > 0)
            throw new ValidationException(failures);
    }

    public static async Task EnsureSkuIsFreeAsync(this IApplicationDbContext context, string sku, CancellationToken cancellationToken)
    {
        var value = SKU.Create(sku);

        if (await context.ProductVariants.AnyAsync(v => v.SKU == value, cancellationToken))
            throw new ValidationException(new[] { new ValidationFailure("Sku", $"SKU '{value}' is already used by another product.") });
    }

    // The stock record for a variant, with its reservations. Sellers keep their stock in
    // Prodify's warehouse, so a new record goes into the first active warehouse.
    public static async Task<InventoryItem?> GetInventoryItemAsync(
        this IApplicationDbContext context, Guid variantId, CancellationToken cancellationToken) =>
        await context.InventoryItems
            .Include(i => i.Reservations)
            .Where(i => i.ProductVariantId == variantId)
            .OrderBy(i => i.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public static async Task<Guid> GetDefaultWarehouseIdAsync(this IApplicationDbContext context, CancellationToken cancellationToken)
    {
        var warehouseId = await context.Warehouses
            .Where(w => w.IsActive == true)
            .OrderBy(w => w.CreatedAt)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return warehouseId
            ?? throw new BusinessRuleException("There is no warehouse to keep stock in yet. Please contact Prodify support.");
    }
}