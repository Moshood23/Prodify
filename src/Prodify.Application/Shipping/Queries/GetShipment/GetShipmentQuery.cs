using MediatR;

namespace Prodify.Application.Shipping.Queries.GetShipment;

public class GetShipmentQuery : IRequest<ShipmentDto>
{
    public Guid ShipmentId { get; set; }
}

public class ShipmentDto
{
    public Guid Id { get; set; }
    public Guid SellerOrderId { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public List<ShipmentItemDto> Items { get; set; } = new();
}

public class ShipmentItemDto
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
}