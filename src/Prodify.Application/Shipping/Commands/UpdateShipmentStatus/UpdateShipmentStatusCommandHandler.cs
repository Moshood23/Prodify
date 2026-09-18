using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Shipping.Commands.UpdateShipmentStatus;

public class UpdateShipmentStatusCommandHandler : IRequestHandler<UpdateShipmentStatusCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateShipmentStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateShipmentStatusCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

        if (shipment is null)
            throw new NotFoundException("Shipment", request.ShipmentId);

        shipment.UpdateStatus(request.Status, request.Description);
    }
}