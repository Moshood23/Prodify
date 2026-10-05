using MediatR;

namespace Prodify.Application.Payouts.Commands.RequestPayout;

// The seller asks for some of their available earnings to be paid to their bank account.
public class RequestPayoutCommand : IRequest<Guid>
{
    public decimal Amount { get; set; }
}
