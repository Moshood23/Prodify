namespace Prodify.Application.Cart.Common;

public static class CartRules
{
    // Stops accidental huge orders; also the most the quantity picker offers.
    public const int MaxQuantityPerItem = 10;
}