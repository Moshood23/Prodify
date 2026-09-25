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
    public DateTime CreatedAt { get; set; }
    public bool IsPaid { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal Total { get; set; }

    // What the customer can do next.
    public bool CanCancel { get; set; }
    public bool CanPay { get; set; }

    public OrderAddressDto ShippingAddress { get; set; } = null!;
    public List<SellerOrderDto> SellerOrders { get; set; } = new();
}

public class OrderAddressDto
{
    public string RecipientName { get; set; } = null!;
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}

public class SellerOrderDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal Total { get; set; }
    public List<SellerOrderItemDto> Items { get; set; } = new();
}

public class SellerOrderItemDto
{
    public Guid ProductVariantId { get; set; }
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}