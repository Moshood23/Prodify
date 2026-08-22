using Prodify.Domain.Common;

namespace Prodify.Domain.Payments.Entities;

public enum PaymentAttemptStatus
{
    Pending,
    Succeeded,
    Failed
}

public class PaymentAttempt : Entity
{
    public Guid PaymentId { get; private set; }
    public PaymentAttemptStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public string? GatewayReference { get; private set; }
    public DateTime AttemptedAt { get; private set; }

    private PaymentAttempt()
    {
    }

    internal PaymentAttempt(Guid id, Guid paymentId) : base(id)
    {
        PaymentId = paymentId;
        Status = PaymentAttemptStatus.Pending;
        AttemptedAt = DateTime.UtcNow;
    }

    internal static PaymentAttempt Create(Guid paymentId)
    {
        return new PaymentAttempt(Guid.NewGuid(), paymentId);
    }

    internal void MarkSucceeded(string gatewayReference)
    {
        if (Status != PaymentAttemptStatus.Pending)
            throw new InvalidOperationException($"Cannot succeed an attempt with status '{Status}'.");

        if (string.IsNullOrWhiteSpace(gatewayReference))
            throw new ArgumentException("Gateway reference cannot be empty.", nameof(gatewayReference));

        Status = PaymentAttemptStatus.Succeeded;
        GatewayReference = gatewayReference.Trim();
    }

    internal void MarkFailed(string? failureReason)
    {
        if (Status != PaymentAttemptStatus.Pending)
            throw new InvalidOperationException($"Cannot fail an attempt with status '{Status}'.");

        Status = PaymentAttemptStatus.Failed;
        FailureReason = failureReason;
    }
}