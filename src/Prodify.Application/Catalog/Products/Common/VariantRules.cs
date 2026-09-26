namespace Prodify.Application.Catalog.Products.Common;

// Limits shared by the "add variant" and "edit variant" forms.
public static class VariantRules
{
    // Letters, numbers and dashes, e.g. "TEC-SPK20-128".
    public const string SkuPattern = "^[A-Za-z0-9][A-Za-z0-9_-]*$";
    public const string SkuMessage = "SKU can only contain letters, numbers, dashes and underscores.";

    public const string CompareAtPriceMessage = "The old price must be higher than the selling price.";

    public const decimal MaxWeightKg = 1000;
    public const int MaxStock = 100_000;
}