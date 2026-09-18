using MediatR;
using Prodify.Domain.Shipping.Entities;

namespace Prodify.Application.Shipping.Commands.UpdateShipmentStatus;

public class UpdateShipmentStatusCommand : IRequest
{
    public Guid ShipmentId { get; set; }
    public ShipmentStatus Status { get; set; }
    public string? Description { get; set; }
}