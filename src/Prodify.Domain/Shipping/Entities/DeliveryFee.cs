using Prodify.Domain.Common;

namespace Prodify.Domain.Shipping.Entities;

// What customers pay for delivery to one state (or the FCT). Admins set it.
public class DeliveryFee : AuditableEntity
{
    public const decimal MaxAmount = 100_000m;

    public string State { get; private set; } = null!;
    public decimal Amount { get; private set; }

    private DeliveryFee()
    {
    }

    private DeliveryFee(Guid id, string state, decimal amount) : base(id)
    {
        State = state;
        Amount = amount;
    }

    public static DeliveryFee Create(string state, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty.", nameof(state));

        var fee = new DeliveryFee(Guid.NewGuid(), state.Trim(), 0);
        fee.ChangeAmount(amount);
        return fee;
    }

    public void ChangeAmount(decimal amount)
    {
        if (amount < 0 || amount > MaxAmount)
            throw new ArgumentOutOfRangeException(nameof(amount), $"Delivery fee must be between 0 and {MaxAmount}.");

        Amount = decimal.Round(amount, 2);
    }
}
