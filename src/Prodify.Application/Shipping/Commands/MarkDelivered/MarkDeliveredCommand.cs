using MediatR;

namespace Prodify.Application.Shipping.Commands.MarkDelivered;

public class MarkDeliveredCommand : IRequest
{
    public Guid ShipmentId { get; set; }
}