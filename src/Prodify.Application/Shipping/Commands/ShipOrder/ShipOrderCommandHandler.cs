using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Shipping.Commands.ShipOrder;

public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public ShipOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

        if (shipment is null)
            throw new NotFoundException("Shipment", request.ShipmentId);

        shipment.Ship(request.Carrier, request.TrackingNumber);
    }
}