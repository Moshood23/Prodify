using Prodify.Domain.Common;

namespace Prodify.Domain.Ordering.Entities;

public enum SellerOrderStatus
{
    Pending,
    Confirmed,
    Packed,
    Shipped,
    Delivered,
    Cancelled
}

public class OrderStatusHistory : Entity
{
    public Guid SellerOrderId { get; private set; }
    public SellerOrderStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private OrderStatusHistory()
    {
    }

    internal OrderStatusHistory(Guid id, Guid sellerOrderId, SellerOrderStatus status, string? notes) : base(id)
    {
        SellerOrderId = sellerOrderId;
        Status = status;
        Notes = notes;
        OccurredAt = DateTime.UtcNow;
    }

    internal static OrderStatusHistory Create(Guid sellerOrderId, SellerOrderStatus status, string? notes = null)
    {
        return new OrderStatusHistory(Guid.NewGuid(), sellerOrderId, status, notes);
    }
}