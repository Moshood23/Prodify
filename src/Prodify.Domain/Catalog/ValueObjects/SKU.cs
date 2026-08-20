using Prodify.Domain.Common;

namespace Prodify.Domain.Catalog.ValueObjects;

public sealed class SKU : ValueObject
{
    public string Value { get; }

    private SKU(string value)
    {
        Value = value;
    }

    public static SKU Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU cannot be empty.", nameof(value));

        if (value.Length > 64)
            throw new ArgumentException("SKU cannot exceed 64 characters.", nameof(value));

        return new SKU(value.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}