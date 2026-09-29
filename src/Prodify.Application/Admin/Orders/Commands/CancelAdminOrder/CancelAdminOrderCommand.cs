using MediatR;

namespace Prodify.Application.Admin.Orders.Commands.CancelAdminOrder;

// An admin stops every part of an order that hasn't left the seller yet;
// if the customer paid by card, that money is refunded.
public class CancelAdminOrderCommand : IRequest
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = null!;
}