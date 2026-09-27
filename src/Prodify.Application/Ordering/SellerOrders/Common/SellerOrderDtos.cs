namespace Prodify.Application.Ordering.SellerOrders.Common;

// One row in the Seller Centre order list.
public class SellerOrderSummaryDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public bool IsPaid { get; set; }
    public string CustomerName { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public int ItemCount { get; set; }
    public string Summary { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal Total { get; set; }
}

// Number of orders at each step, for the tabs and the dashboard.
public class SellerOrderCountsDto
{
    public int ToConfirm { get; set; }
    public int ToPack { get; set; }
    public int ToShip { get; set; }
    public int Shipped { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }
}

public class SellerOrderDetailsDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public bool IsPaid { get; set; }
    public decimal Total { get; set; }

    public SellerOrderAddressDto ShippingAddress { get; set; } = null!;
    public List<SellerOrderLineDto> Items { get; set; } = new();
    public SellerOrderShipmentDto? Shipment { get; set; }
    public List<SellerOrderHistoryDto> History { get; set; } = new();

    // Which buttons to show.
    public bool CanConfirm { get; set; }
    public bool CanPack { get; set; }
    public bool CanShip { get; set; }
    public bool CanDeliver { get; set; }
    public bool CanCancel { get; set; }
}

public class SellerOrderAddressDto
{
    public string RecipientName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
}

public class SellerOrderLineDto
{
    public Guid ProductVariantId { get; set; }
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Sku { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

public class SellerOrderShipmentDto
{
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

public class SellerOrderHistoryDto
{
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime OccurredAt { get; set; }
}