using MediatR;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Admin.Reviews.Queries.ListAdminReviews;

// Every review, newest first, for admins to check.
public class ListAdminReviewsQuery : IRequest<PaginatedList<AdminReviewDto>>
{
    // Product name, reviewer name or words in the review.
    public string? Search { get; set; }

    // 1 to 5, or empty for all.
    public int? Rating { get; set; }

    // "visible", "hidden" or empty for all.
    public string? Status { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminReviewDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public string ReviewerName { get; set; } = null!;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsHidden { get; set; }
    public string? HiddenReason { get; set; }
}
