using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Shipping.Common;
using Prodify.Domain.Shipping.Entities;

namespace Prodify.Application.Shipping.Commands.MarkDelivered;

public class MarkDeliveredCommandHandler : IRequestHandler<MarkDeliveredCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkDeliveredCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkDeliveredCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

        if (shipment is null
            || !await ShipmentAccess.CanManageAsync(_context, _currentUser, shipment.SellerOrderId, cancellationToken))
            throw new NotFoundException("Shipment", request.ShipmentId);

        shipment.UpdateStatus(ShipmentStatus.Delivered, "Package delivered");
    }
}