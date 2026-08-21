using Prodify.Domain.Common;
using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.Domain.Ordering.Entities;

public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid SellerOrderId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;

    public Money Subtotal => UnitPrice.Multiply(Quantity);

    private OrderItem()
    {
    }

    internal OrderItem(
        Guid id,
        Guid orderId,
        Guid sellerOrderId,
        Guid productVariantId,
        string productName,
        int quantity,
        Money unitPrice) : base(id)
    {
        OrderId = orderId;
        SellerOrderId = sellerOrderId;
        ProductVariantId = productVariantId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal static OrderItem Create(
        Guid orderId,
        Guid sellerOrderId,
        Guid productVariantId,
        string productName,
        int quantity,
        Money unitPrice)
    {
        return new OrderItem(Guid.NewGuid(), orderId, sellerOrderId, productVariantId, productName, quantity, unitPrice);
    }
}