using Prodify.Domain.Common;
using Prodify.Domain.Ordering.ValueObjects;
using Prodify.Domain.Payments.Events;

namespace Prodify.Domain.Payments.Entities;

public enum PaymentStatus
{
    Pending,
    Processing,
    Succeeded,
    Failed
}

public class Payment : AuditableEntity
{
    private readonly List<PaymentAttempt> _attempts = new();

    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }

    public IReadOnlyCollection<PaymentAttempt> Attempts => _attempts.AsReadOnly();

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
        if (Status == PaymentStatus.Succeeded)
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

    private PaymentAttempt GetAttemptOrThrow(Guid attemptId)
    {
        var attempt = _attempts.FirstOrDefault(a => a.Id == attemptId);

        if (attempt is null)
            throw new InvalidOperationException($"Payment attempt '{attemptId}' not found.");

        return attempt;
    }
}