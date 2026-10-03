namespace Prodify.Application.Cart.Common;

// One line of a guest's cart, which lives in their browser until they log in.
public class GuestCartItem
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
}
