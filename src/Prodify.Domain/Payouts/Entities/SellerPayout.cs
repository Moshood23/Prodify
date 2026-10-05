using Prodify.Domain.Common;

namespace Prodify.Domain.Payouts.Entities;

public enum PayoutStatus
{
    Requested,
    Paid,
    Rejected
}

// A seller asking for their available earnings to be sent to their bank.
// An admin makes the transfer, then marks it paid (or rejects it).
public class SellerPayout : AuditableEntity
{
    public const decimal MinimumAmount = 5_000m;

    public Guid SellerId { get; private set; }
    public decimal Amount { get; private set; }
    public PayoutStatus Status { get; private set; }

    // The bank details at the time of the request.
    public string BankName { get; private set; } = null!;
    public string AccountNumber { get; private set; } = null!;
    public string AccountName { get; private set; } = null!;

    // The bank transfer reference, once paid.
    public string? Reference { get; private set; }
    public string? RejectReason { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private SellerPayout()
    {
    }

    private SellerPayout(Guid id) : base(id)
    {
    }

    public static SellerPayout Request(Guid sellerId, decimal amount, PayoutAccount account)
    {
        if (amount < MinimumAmount)
            throw new ArgumentOutOfRangeException(nameof(amount), $"The smallest payout is {MinimumAmount:N0}.");
        if (account.SellerId != sellerId)
            throw new InvalidOperationException("The bank account belongs to another seller.");

        return new SellerPayout(Guid.NewGuid())
        {
            SellerId = sellerId,
            Amount = Math.Round(amount, 2),
            Status = PayoutStatus.Requested,
            BankName = account.BankName,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
        };
    }

    public void MarkPaid(string reference)
    {
        EnsureRequested();
        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("Give the bank transfer reference.", nameof(reference));

        Status = PayoutStatus.Paid;
        Reference = reference.Trim();
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        EnsureRequested();
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Give a reason for rejecting the payout.", nameof(reason));

        Status = PayoutStatus.Rejected;
        RejectReason = reason.Trim();
        ProcessedAt = DateTime.UtcNow;
    }

    private void EnsureRequested()
    {
        if (Status != PayoutStatus.Requested)
            throw new InvalidOperationException($"This payout is already {Status.ToString().ToLowerInvariant()}.");
    }
}
