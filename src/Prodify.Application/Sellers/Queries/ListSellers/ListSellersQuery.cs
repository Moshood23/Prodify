using MediatR;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Sellers.Queries.ListSellers;

// Admin: all sellers, optionally filtered by status ("PendingVerification", "Approved", ...) or a search term.
public class ListSellersQuery : IRequest<PaginatedList<SellerSummaryDto>>
{
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SellerSummaryDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string Status { get; set; } = null!;
    public string? City { get; set; }
    public string? State { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StatusChangedAt { get; set; }
    public int ProductCount { get; set; }
}