using MediatR;

namespace Prodify.Application.Cart.Queries.GetCart;

public class GetCartQuery : IRequest<CartDto>
{
}

public class CartDto
{
    public Guid? Id { get; set; }

    // Priced at today's prices, not the price when the item was added.
    public decimal Total { get; set; }
    public int ItemCount { get; set; }

    // True when something in the cart can't be bought right now (removed or out of stock);
    // checkout is blocked until it is fixed.
    public bool HasProblems { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? VariantName { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public int AvailableQuantity { get; set; }

    // Available = can be bought, InStock = enough units for the quantity in the cart.
    public bool IsAvailable { get; set; }
    public bool InStock { get; set; }
}