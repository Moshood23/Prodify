using MediatR;

namespace Prodify.Application.Shipping.Commands.ShipOrder;

public class ShipOrderCommand : IRequest
{
    public Guid ShipmentId { get; set; }
    public string Carrier { get; set; } = null!;
    public string TrackingNumber { get; set; } = null!;
}