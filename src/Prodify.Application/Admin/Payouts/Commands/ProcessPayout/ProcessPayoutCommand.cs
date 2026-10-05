using MediatR;

namespace Prodify.Application.Admin.Payouts.Commands.ProcessPayout;

// An admin records that a payout was sent (with the bank reference) or rejects it (with a reason).
public class ProcessPayoutCommand : IRequest
{
    public Guid PayoutId { get; set; }
    public bool Paid { get; set; }
    public string? Reference { get; set; }
    public string? Reason { get; set; }
}
