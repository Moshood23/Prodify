using Prodify.Domain.Common;

namespace Prodify.Domain.Shipping.Entities;

public class ShipmentItem : Entity
{
    public Guid ShipmentId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }

    private ShipmentItem() { }

    internal ShipmentItem(Guid id, Guid shipmentId, Guid productVariantId, int quantity) : base(id)
    {
        ShipmentId = shipmentId;
        ProductVariantId = productVariantId;
        Quantity = quantity;
    }

    internal static ShipmentItem Create(Guid shipmentId, Guid productVariantId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        return new ShipmentItem(Guid.NewGuid(), shipmentId, productVariantId, quantity);
    }
}