using Prodify.Domain.Common;

namespace Prodify.Domain.Customers.Entities;

public enum StoreCreditKind
{
    // Money given back as credit: a cancelled order paid with credit, or a returned cash order.
    Refund,
    // Credit used to pay for an order.
    Spent
}

// One movement in a customer's store credit. The balance is the sum of all their entries.
public class StoreCreditEntry : AuditableEntity
{
    public Guid CustomerId { get; private set; }

    // Positive when credit is added, negative when it is spent.
    public decimal Amount { get; private set; }
    public StoreCreditKind Kind { get; private set; }
    public string Description { get; private set; } = null!;
    public Guid? OrderId { get; private set; }

    private StoreCreditEntry()
    {
    }

    private StoreCreditEntry(Guid id) : base(id)
    {
    }

    public static StoreCreditEntry Add(Guid customerId, decimal amount, string description, Guid? orderId) =>
        Create(customerId, amount, StoreCreditKind.Refund, description, orderId);

    public static StoreCreditEntry Spend(Guid customerId, decimal amount, string description, Guid orderId) =>
        Create(customerId, -amount, StoreCreditKind.Spent, description, orderId);

    private static StoreCreditEntry Create(Guid customerId, decimal signedAmount, StoreCreditKind kind, string description, Guid? orderId)
    {
        if (signedAmount == 0 || Math.Round(signedAmount, 2) != signedAmount)
            throw new ArgumentOutOfRangeException(nameof(signedAmount), "Store credit must be a non-zero amount in naira and kobo.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Say what the credit is for.", nameof(description));

        return new StoreCreditEntry(Guid.NewGuid())
        {
            CustomerId = customerId,
            Amount = signedAmount,
            Kind = kind,
            Description = description.Trim(),
            OrderId = orderId,
        };
    }
}
