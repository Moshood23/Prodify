using MediatR;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Admin.Orders.Queries.ListAdminOrders;

// Every order on the platform, newest first.
public class ListAdminOrdersQuery : IRequest<PaginatedList<AdminOrderSummaryDto>>
{
    // awaiting_payment, in_progress, delivered or cancelled; empty for all.
    public string? Stage { get; set; }

    // Card or PayOnDelivery.
    public string? PaymentMethod { get; set; }

    // Order number, or the customer's name or email.
    public string? Search { get; set; }

    public Guid? CustomerId { get; set; }

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminOrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = null!;
    public bool IsPaid { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public string Summary { get; set; } = null!;
    public string? ImageUrl { get; set; }

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public int SellerCount { get; set; }
}