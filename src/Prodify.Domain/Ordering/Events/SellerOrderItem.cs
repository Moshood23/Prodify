using Prodify.Domain.Common;
using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.Domain.Ordering.Entities;

public class SellerOrderItem : Entity
{
    public Guid SellerOrderId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;

    public Money Subtotal => UnitPrice.Multiply(Quantity);

    private SellerOrderItem()
    {
    }

    internal SellerOrderItem(
        Guid id,
        Guid sellerOrderId,
        Guid productVariantId,
        string productName,
        int quantity,
        Money unitPrice) : base(id)
    {
        SellerOrderId = sellerOrderId;
        ProductVariantId = productVariantId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal static SellerOrderItem Create(
        Guid sellerOrderId,
        Guid productVariantId,
        string productName,
        int quantity,
        Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty.", nameof(productName));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        return new SellerOrderItem(Guid.NewGuid(), sellerOrderId, productVariantId, productName.Trim(), quantity, unitPrice);
    }
}