using Prodify.Application.Catalog.Products.DTOs;

namespace Prodify.Application.Catalog.Products.Common;

public static class ManagedProductStatusRules
{
    public static string For(bool isActive, int activeVariantCount, int availableStock) =>
        !isActive ? ManagedProductStatus.Inactive
        : activeVariantCount == 0 ? ManagedProductStatus.Incomplete
        : availableStock == 0 ? ManagedProductStatus.OutOfStock
        : ManagedProductStatus.Live;
}