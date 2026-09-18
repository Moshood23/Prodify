using MediatR;

namespace Prodify.Application.Shipping.Queries.GetTracking;

public class GetTrackingQuery : IRequest<List<TrackingEventDto>>
{
    public Guid ShipmentId { get; set; }
}

public class TrackingEventDto
{
    public string Status { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime OccurredAt { get; set; }
}