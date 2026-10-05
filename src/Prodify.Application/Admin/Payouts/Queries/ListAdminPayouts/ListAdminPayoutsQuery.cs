using MediatR;
using Prodify.Application.Common.Models;
using Prodify.Application.Payouts.Common;

namespace Prodify.Application.Admin.Payouts.Queries.ListAdminPayouts;

// Payout requests for admins, oldest waiting first when filtering by "Requested".
public class ListAdminPayoutsQuery : IRequest<PaginatedList<AdminPayoutDto>>
{
    // Requested, Paid, Rejected or empty for all.
    public string? Status { get; set; }

    // Store name or account name.
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminPayoutDto : PayoutDto
{
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = null!;
    public string SellerEmail { get; set; } = null!;
}
