using Prodify.Domain.Common;

namespace Prodify.Domain.Ordering.ValueObjects;

public sealed class OrderNumber : ValueObject
{
    public string Value { get; }

    private OrderNumber(string value)
    {
        Value = value;
    }

    public static OrderNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Order number cannot be empty.", nameof(value));

        return new OrderNumber(value.Trim());
    }

    public static OrderNumber Generate()
    {
        var timestamp = DateTime.UtcNow;
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        return new OrderNumber($"ORD-{timestamp:yyyyMMdd}-{suffix}");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}