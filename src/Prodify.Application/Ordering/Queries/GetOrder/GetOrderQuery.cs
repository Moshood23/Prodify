using MediatR;

namespace Prodify.Application.Ordering.Queries.GetOrder;

public class GetOrderQuery : IRequest<OrderDto>
{
    public Guid Id { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public bool IsPaid { get; set; }
    public string Status { get; set; } = null!;
    public decimal Total { get; set; }
    public List<SellerOrderDto> SellerOrders { get; set; } = new();
}

public class SellerOrderDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string Status { get; set; } = null!;
    public decimal Total { get; set; }
    public List<SellerOrderItemDto> Items { get; set; } = new();
}

public class SellerOrderItemDto
{
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}