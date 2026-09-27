using MediatR;
using Prodify.Application.Common.Models;
using Prodify.Application.Ordering.SellerOrders.Common;

namespace Prodify.Application.Ordering.SellerOrders.Queries.ListSellerOrders;

// Seller Centre order list. Sellers always see their own orders; admins pass the seller.
public class ListSellerOrdersQuery : IRequest<PaginatedList<SellerOrderSummaryDto>>
{
    public Guid? SellerId { get; set; }

    // Pending, Confirmed, Packed, Shipped, Delivered or Cancelled; empty for all.
    public string? Status { get; set; }

    // Part of the order number.
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}