using Prodify.Domain.Common;
using Prodify.Domain.Ordering.Events;
using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.Domain.Ordering.Entities;

public enum OrderStatus
{
    Pending,
    Confirmed,
    PartiallyShipped,
    Shipped,
    PartiallyDelivered,
    Delivered,
    Cancelled
}

public class Order : AuditableEntity
{
    private readonly List<SellerOrder> _sellerOrders = new();
    private readonly List<OrderItem> _items = new();

    public OrderNumber OrderNumber { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public OrderAddress ShippingAddress { get; private set; } = null!;
    public bool IsPaid { get; private set; }

    public IReadOnlyCollection<SellerOrder> SellerOrders => _sellerOrders.AsReadOnly();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Money Total => _items
        .Select(i => i.Subtotal)
        .Aggregate(Money.Zero(), (acc, next) => acc.Add(next));

    public OrderStatus Status => ComputeStatus();

    private Order()
    {
    }

    private Order(Guid id, Guid customerId, OrderAddress shippingAddress) : base(id)
    {
        OrderNumber = OrderNumber.Generate();
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        IsPaid = false;
    }

    public static Order Place(Guid customerId, OrderAddress shippingAddress, IEnumerable<(Guid SellerId, Guid ProductVariantId, string ProductName, int Quantity, Money UnitPrice)> lineItems)
    {
        var order = new Order(Guid.NewGuid(), customerId, shippingAddress);

        var groupedBySeller = lineItems.GroupBy(li => li.SellerId);

        foreach (var sellerGroup in groupedBySeller)
        {
            var sellerOrder = SellerOrder.Create(order.Id, sellerGroup.Key);

            foreach (var line in sellerGroup)
            {
                sellerOrder.AddItem(line.ProductVariantId, line.ProductName, line.Quantity, line.UnitPrice);

                var orderItem = OrderItem.Create(
                    order.Id,
                    sellerOrder.Id,
                    line.ProductVariantId,
                    line.ProductName,
                    line.Quantity,
                    line.UnitPrice);

                order._items.Add(orderItem);
            }

            order._sellerOrders.Add(sellerOrder);
        }

        if (!order._items.Any())
            throw new InvalidOperationException("An order must contain at least one item.");

        order.AddDomainEvent(new OrderPlacedEvent(order.Id, order.CustomerId));

        return order;
    }

    public void MarkAsPaid(Guid paymentId)
    {
        if (IsPaid)
            throw new InvalidOperationException("Order is already paid.");

        IsPaid = true;
        AddDomainEvent(new OrderPaidEvent(Id, paymentId));
    }

    public void Cancel(string? reason = null)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.PartiallyDelivered or OrderStatus.Delivered)
            throw new InvalidOperationException($"Cannot cancel an order with status '{Status}'.");

        foreach (var sellerOrder in _sellerOrders)
        {
            if (sellerOrder.Status is SellerOrderStatus.Pending or SellerOrderStatus.Confirmed)
                sellerOrder.TransitionTo(SellerOrderStatus.Cancelled, reason);
        }

        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }

    public void MarkSellerOrderDelivered(Guid sellerOrderId)
    {
        var sellerOrder = _sellerOrders.FirstOrDefault(so => so.Id == sellerOrderId);

        if (sellerOrder is null)
            throw new InvalidOperationException($"SellerOrder '{sellerOrderId}' not found on this order.");

        sellerOrder.TransitionTo(SellerOrderStatus.Delivered);
        AddDomainEvent(new OrderDeliveredEvent(Id, sellerOrderId));
    }

    private OrderStatus ComputeStatus()
    {
        if (_sellerOrders.All(so => so.Status == SellerOrderStatus.Cancelled))
            return OrderStatus.Cancelled;

        if (_sellerOrders.All(so => so.Status == SellerOrderStatus.Delivered))
            return OrderStatus.Delivered;

        if (_sellerOrders.Any(so => so.Status == SellerOrderStatus.Delivered))
            return OrderStatus.PartiallyDelivered;

        if (_sellerOrders.All(so => so.Status == SellerOrderStatus.Shipped))
            return OrderStatus.Shipped;

        if (_sellerOrders.Any(so => so.Status == SellerOrderStatus.Shipped))
            return OrderStatus.PartiallyShipped;

        if (_sellerOrders.All(so => so.Status == SellerOrderStatus.Pending))
            return OrderStatus.Pending;

        return OrderStatus.Confirmed;
    }
}