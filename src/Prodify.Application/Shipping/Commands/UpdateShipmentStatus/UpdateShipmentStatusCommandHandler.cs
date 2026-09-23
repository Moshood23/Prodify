using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Shipping.Common;

namespace Prodify.Application.Shipping.Commands.UpdateShipmentStatus;

public class UpdateShipmentStatusCommandHandler : IRequestHandler<UpdateShipmentStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateShipmentStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateShipmentStatusCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

        if (shipment is null
            || !await ShipmentAccess.CanManageAsync(_context, _currentUser, shipment.SellerOrderId, cancellationToken))
            throw new NotFoundException("Shipment", request.ShipmentId);

        shipment.UpdateStatus(request.Status, request.Description);
    }
}