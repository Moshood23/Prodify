using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Shipping.Entities;

namespace Prodify.Application.Shipping.Commands.MarkDelivered;

public class MarkDeliveredCommandHandler : IRequestHandler<MarkDeliveredCommand>
{
    private readonly IApplicationDbContext _context;

    public MarkDeliveredCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MarkDeliveredCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

        if (shipment is null)
            throw new NotFoundException("Shipment", request.ShipmentId);

        shipment.UpdateStatus(ShipmentStatus.Delivered, "Package delivered");
    }
}