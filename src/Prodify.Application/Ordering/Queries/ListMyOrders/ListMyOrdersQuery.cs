using MediatR;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Ordering.Queries.ListMyOrders;

public class ListMyOrdersQuery : IRequest<PaginatedList<OrderSummaryDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class OrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = null!;
    public bool IsPaid { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public decimal Total { get; set; }
    public int ItemCount { get; set; }

    // e.g. "Tecno Spark 20 Pro (8GB RAM / 128GB) and 2 more"
    public string Summary { get; set; } = null!;
    public string? ImageUrl { get; set; }
}