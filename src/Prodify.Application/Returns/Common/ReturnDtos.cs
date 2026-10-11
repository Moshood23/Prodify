namespace Prodify.Application.Returns.Common;

public class ReturnDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public Guid SellerOrderId { get; set; }
    public string SellerName { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string? Details { get; set; }
    public string Status { get; set; } = null!;
    public string? SellerNote { get; set; }
    public string? CustomerNote { get; set; }
    public string? AdminNote { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal RefundedToCard { get; set; }
    public decimal RefundedAsCredit { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public List<ReturnItemDto> Items { get; set; } = new();
}

public class ReturnItemDto
{
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
