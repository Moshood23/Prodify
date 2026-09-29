using Prodify.Domain.Common;
using Prodify.Domain.Ordering.ValueObjects;
using Prodify.Domain.Payments.Events;

namespace Prodify.Domain.Payments.Entities;

public enum PaymentStatus
{
    Pending,
    Processing,
    Succeeded,
    Failed,
    PartiallyRefunded,
    Refunded
}

public class Payment : AuditableEntity
{
    private readonly List<PaymentAttempt> _attempts = new();

    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }

    // Money sent back to the customer so far (cancelled orders or parts of orders).
    public decimal RefundedAmount { get; private set; }

    public bool IsCaptured => Status is PaymentStatus.Succeeded or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded;

    // The gateway's reference for the successful charge; refunds are made against it.
    public string? GatewayReference => _attempts
        .Where(a => a.Status == PaymentAttemptStatus.Succeeded)
        .Select(a => a.GatewayReference)
        .FirstOrDefault();

    public ICollection<PaymentAttempt> Attempts => _attempts;
    private Payment()
    {
    }

    private Payment(Guid id, Guid orderId, Money amount) : base(id)
    {
        OrderId = orderId;
        Amount = amount;
        Status = PaymentStatus.Pending;
    }

    public static Payment Create(Guid orderId, Money amount)
    {
        return new Payment(Guid.NewGuid(), orderId, amount);
    }

    public PaymentAttempt StartAttempt()
    {
        if (IsCaptured)
            throw new InvalidOperationException("Cannot start a new attempt for an already-succeeded payment.");

        var attempt = PaymentAttempt.Create(Id);
        _attempts.Add(attempt);
        Status = PaymentStatus.Processing;

        return attempt;
    }

    public void CompleteAttempt(Guid attemptId, string gatewayReference)
    {
        var attempt = GetAttemptOrThrow(attemptId);

        attempt.MarkSucceeded(gatewayReference);
        Status = PaymentStatus.Succeeded;

        AddDomainEvent(new PaymentSuccessfulEvent(Id, OrderId));
    }

    public void FailAttempt(Guid attemptId, string? reason)
    {
        var attempt = GetAttemptOrThrow(attemptId);

        attempt.MarkFailed(reason);
        Status = PaymentStatus.Failed;

        AddDomainEvent(new PaymentFailedEvent(Id, OrderId, reason));
    }

    public void RecordRefund(decimal amount)
    {
        if (!IsCaptured)
            throw new InvalidOperationException("Only a successful payment can be refunded.");

        if (amount <= 0)
            throw new ArgumentException("Refund amount must be greater than zero.", nameof(amount));

        if (RefundedAmount + amount > Amount.Amount)
            throw new InvalidOperationException("Cannot refund more than was paid.");

        RefundedAmount += amount;
        Status = RefundedAmount == Amount.Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
    }

    private PaymentAttempt GetAttemptOrThrow(Guid attemptId)
    {
        var attempt = _attempts.FirstOrDefault(a => a.Id == attemptId);

        if (attempt is null)
            throw new InvalidOperationException($"Payment attempt '{attemptId}' not found.");

        return attempt;
    }
}