using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Shipping.Queries.GetShipment;

public class GetShipmentQueryHandler : IRequestHandler<GetShipmentQuery, ShipmentDto>
{
    private readonly IApplicationDbContext _context;

    public GetShipmentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ShipmentDto> Handle(GetShipmentQuery request, CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

        if (shipment is null)
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