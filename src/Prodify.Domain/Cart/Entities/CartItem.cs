using Prodify.Domain.Common;

namespace Prodify.Domain.Cart.Entities;

public class CartItem : Entity
{
    public Guid CartId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private CartItem()
    {
    }

    internal CartItem(Guid id, Guid cartId, Guid productVariantId, int quantity, decimal unitPrice) : base(id)
    {
        CartId = cartId;
        ProductVariantId = productVariantId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal static CartItem Create(Guid cartId, Guid productVariantId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        return new CartItem(Guid.NewGuid(), cartId, productVariantId, quantity, unitPrice);
    }

    internal void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Quantity = quantity;
    }

    public decimal Subtotal => UnitPrice * Quantity;
}