using Prodify.Domain.Common;
using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.Domain.Ordering.Entities;

public class SellerOrder : AuditableEntity
{
    private readonly List<SellerOrderItem> _items = new();
    private readonly List<OrderStatusHistory> _statusHistory = new();

    public Guid OrderId { get; private set; }
    public Guid SellerId { get; private set; }
    public SellerOrderStatus Status { get; private set; }

    public IReadOnlyCollection<SellerOrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    public Money Total => _items
        .Select(i => i.Subtotal)
        .Aggregate(Money.Zero(), (acc, next) => acc.Add(next));

    private SellerOrder()
    {
    }

    private SellerOrder(Guid id, Guid orderId, Guid sellerId) : base(id)
    {
        OrderId = orderId;
        SellerId = sellerId;
        Status = SellerOrderStatus.Pending;
    }

    public static SellerOrder Create(Guid orderId, Guid sellerId)
    {
        var sellerOrder = new SellerOrder(Guid.NewGuid(), orderId, sellerId);
        sellerOrder._statusHistory.Add(OrderStatusHistory.Create(sellerOrder.Id, SellerOrderStatus.Pending));

        return sellerOrder;
    }

    public SellerOrderItem AddItem(Guid productVariantId, string productName, int quantity, Money unitPrice)
    {
        var item = SellerOrderItem.Create(Id, productVariantId, productName, quantity, unitPrice);
        _items.Add(item);

        return item;
    }

    public void TransitionTo(SellerOrderStatus newStatus, string? notes = null)
    {
        if (!IsValidTransition(Status, newStatus))
            throw new InvalidOperationException($"Cannot transition from '{Status}' to '{newStatus}'.");

        Status = newStatus;
        _statusHistory.Add(OrderStatusHistory.Create(Id, newStatus, notes));
    }

    private static bool IsValidTransition(SellerOrderStatus current, SellerOrderStatus next)
    {
        if (current == SellerOrderStatus.Cancelled || current == SellerOrderStatus.Delivered)
            return false;

        return next switch
        {
            SellerOrderStatus.Confirmed => current == SellerOrderStatus.Pending,
            SellerOrderStatus.Packed => current == SellerOrderStatus.Confirmed,
            SellerOrderStatus.Shipped => current == SellerOrderStatus.Packed,
            SellerOrderStatus.Delivered => current == SellerOrderStatus.Shipped,
            SellerOrderStatus.Cancelled => current is SellerOrderStatus.Pending or SellerOrderStatus.Confirmed,
            _ => false
        };
    }
}