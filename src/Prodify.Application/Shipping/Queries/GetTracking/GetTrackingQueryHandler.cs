using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Shipping.Queries.GetTracking;

public class GetTrackingQueryHandler : IRequestHandler<GetTrackingQuery, List<TrackingEventDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTrackingQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TrackingEventDto>> Handle(GetTrackingQuery request, CancellationToken cancellationToken)
    {
        var shipment = await _context.Shipments
            .Include(s => s.TrackingEvents)
            .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

        if (shipment is null)
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