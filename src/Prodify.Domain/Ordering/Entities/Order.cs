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

public enum PaymentMethod
{
    Card,
    PayOnDelivery
}

public class Order : AuditableEntity
{
    private readonly List<SellerOrder> _sellerOrders = new();
    private readonly List<OrderItem> _items = new();

    public OrderNumber OrderNumber { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public OrderAddress ShippingAddress { get; private set; } = null!;
    public bool IsPaid { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }

    // Fixed when the order is placed, so later changes to the state's fee don't affect it.
    public decimal DeliveryFee { get; private set; }

    // A voucher used at checkout: its code and the naira it took off the items (Prodify pays it).
    public string? VoucherCode { get; private set; }
    public decimal Discount { get; private set; }

    public ICollection<SellerOrder> SellerOrders => _sellerOrders;
    public ICollection<OrderItem> Items => _items;

    public Money ItemsTotal => _items
        .Select(i => i.Subtotal)
        .Aggregate(Money.Zero(), (acc, next) => acc.Add(next));

    // What the customer pays: items plus delivery, minus any voucher.
    public Money Total => Money.Create(ItemsTotal.Amount + DeliveryFee - Discount);

    public OrderStatus Status => ComputeStatus();

    private Order()
    {
    }

    private Order(Guid id, Guid customerId, OrderAddress shippingAddress, PaymentMethod paymentMethod, decimal deliveryFee) : base(id)
    {
        OrderNumber = OrderNumber.Generate();
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        PaymentMethod = paymentMethod;
        DeliveryFee = deliveryFee;
        IsPaid = false;
    }

    public static Order Place(
        Guid customerId,
        OrderAddress shippingAddress,
        IEnumerable<(Guid SellerId, Guid ProductVariantId, string ProductName, int Quantity, Money UnitPrice)> lineItems,
        PaymentMethod paymentMethod = PaymentMethod.Card,
        decimal deliveryFee = 0)
    {
        if (deliveryFee < 0)
            throw new ArgumentOutOfRangeException(nameof(deliveryFee), "Delivery fee can't be negative.");

        var order = new Order(Guid.NewGuid(), customerId, shippingAddress, paymentMethod, deliveryFee);

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

    public void ApplyVoucher(string code, decimal discount)
    {
        if (VoucherCode is not null)
            throw new InvalidOperationException("This order already has a voucher.");

        if (discount <= 0 || discount > ItemsTotal.Amount)
            throw new ArgumentOutOfRangeException(nameof(discount), "A voucher discount must be more than 0 and no more than the items.");

        VoucherCode = code;
        Discount = discount;
    }

    // The part of the voucher discount that belongs to these seller parts, in proportion to their items.
    // Used so a refund or cash collection for some parts gives back only their share of the discount.
    // Works from the seller parts (all of them, cancelled too), which are loaded wherever parts change.
    public decimal DiscountShare(IEnumerable<SellerOrder> parts)
    {
        var allParts = _sellerOrders.Sum(so => so.Total.Amount);
        if (Discount == 0 || allParts == 0)
            return 0;

        var partsTotal = parts.Sum(p => p.Total.Amount);
        return Math.Round(Discount * partsTotal / allParts, 2, MidpointRounding.AwayFromZero);
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
            if (sellerOrder.Status is SellerOrderStatus.Pending or SellerOrderStatus.Confirmed or SellerOrderStatus.Packed)
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
        // Parts a seller cancelled don't hold the rest of the order back:
        // one part delivered + one part cancelled means the order is delivered.
        var active = _sellerOrders.Where(so => so.Status != SellerOrderStatus.Cancelled).ToList();

        if (active.Count == 0)
            return OrderStatus.Cancelled;

        if (active.All(so => so.Status == SellerOrderStatus.Delivered))
            return OrderStatus.Delivered;

        if (active.Any(so => so.Status == SellerOrderStatus.Delivered))
            return OrderStatus.PartiallyDelivered;

        if (active.All(so => so.Status == SellerOrderStatus.Shipped))
            return OrderStatus.Shipped;

        if (active.Any(so => so.Status == SellerOrderStatus.Shipped))
            return OrderStatus.PartiallyShipped;

        if (active.All(so => so.Status == SellerOrderStatus.Pending))
            return OrderStatus.Pending;

        return OrderStatus.Confirmed;
    }
}
