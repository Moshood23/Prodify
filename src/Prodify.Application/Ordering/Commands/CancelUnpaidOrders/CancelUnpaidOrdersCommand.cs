using MediatR;

namespace Prodify.Application.Ordering.Commands.CancelUnpaidOrders;

// Run every minute by a background job: card orders that weren't paid within
// the payment window are cancelled. Returns how many were cancelled.
public class CancelUnpaidOrdersCommand : IRequest<int>
{
    public DateTime Now { get; set; } = DateTime.UtcNow;
}