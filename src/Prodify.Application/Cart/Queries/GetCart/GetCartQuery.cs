using MediatR;

namespace Prodify.Application.Cart.Queries.GetCart;

public class GetCartQuery : IRequest<CartDto>
{
    public Guid? CustomerId { get; set; }
    public string? SessionId { get; set; }
}

public class CartDto
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}