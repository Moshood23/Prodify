using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Shipping.Common;

namespace Prodify.Application.Shipping.Queries.GetShipment;

public class GetShipmentQueryHandler : IRequestHandler<GetShipmentQuery, ShipmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetShipmentQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ShipmentDto> Handle(GetShipmentQuery request, CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

        if (shipment is null
            || !await ShipmentAccess.CanViewAsync(_context, _currentUser, shipment.SellerOrderId, cancellationToken))
            throw new NotFoundException("Shipment", request.ShipmentId);

        return new ShipmentDto
        {
            Id = shipment.Id,
            SellerOrderId = shipment.SellerOrderId,
            Carrier = shipment.Carrier,
            TrackingNumber = shipment.TrackingNumber,
            Status = shipment.Status.ToString(),
            ShippedAt = shipment.ShippedAt,
            DeliveredAt = shipment.DeliveredAt,
            Items = shipment.Items.Select(i => new ShipmentItemDto
            {
                ProductVariantId = i.ProductVariantId,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}