using MediatR;

namespace Prodify.Application.Shipping.Commands.CreateShipment;

public class CreateShipmentCommand : IRequest<Guid>
{
    public Guid SellerOrderId { get; set; }
}