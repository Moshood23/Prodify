namespace Prodify.Application.Payouts.Common;

public class PayoutAccountDto
{
    public string BankName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string AccountName { get; set; } = null!;
}

public class PayoutDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = null!;
    public string BankName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public string? Reference { get; set; }
    public string? RejectReason { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
