using MediatR;

namespace Prodify.Application.Ordering.Commands.CancelOrder;

public class CancelOrderCommand : IRequest
{
    public Guid OrderId { get; set; }
    public string? Reason { get; set; }
}