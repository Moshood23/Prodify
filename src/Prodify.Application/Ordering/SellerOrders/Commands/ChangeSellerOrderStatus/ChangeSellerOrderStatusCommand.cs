using MediatR;

namespace Prodify.Application.Ordering.SellerOrders.Commands.ChangeSellerOrderStatus;

// Moves the seller's part of an order to its next step.
public class ChangeSellerOrderStatusCommand : IRequest
{
    public Guid SellerOrderId { get; set; }

    // Confirmed, Packed, Shipped, Delivered or Cancelled.
    public string Status { get; set; } = null!;

    // Shipped: who is delivering it, e.g. "GIG Logistics" or "Own rider".
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }

    // Cancelled: why, shown to the customer.
    public string? Reason { get; set; }
}