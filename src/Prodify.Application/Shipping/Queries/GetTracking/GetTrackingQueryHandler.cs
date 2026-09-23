using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Shipping.Common;

namespace Prodify.Application.Shipping.Queries.GetTracking;

public class GetTrackingQueryHandler : IRequestHandler<GetTrackingQuery, List<TrackingEventDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTrackingQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TrackingEventDto>> Handle(GetTrackingQuery request, CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .Include(s => s.TrackingEvents)
            .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

        if (shipment is null
            || !await ShipmentAccess.CanViewAsync(_context, _currentUser, shipment.SellerOrderId, cancellationToken))
            throw new NotFoundException("Shipment", request.ShipmentId);

        return shipment.TrackingEvents
            .OrderBy(t => t.OccurredAt)
            .Select(t => new TrackingEventDto
            {
                Status = t.Status.ToString(),
                Description = t.Description,
                OccurredAt = t.OccurredAt
            })
            .ToList();
    }
}