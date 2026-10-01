using MediatR;
using Prodify.Application.Catalog.Reviews.Common;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Catalog.Reviews.Queries.GetProductReviews;

// The reviews section of a product page. Anyone can read it; a logged-in
// customer also gets their own review and whether they may write one.
public class GetProductReviewsQuery : IRequest<ProductReviewsDto>
{
    public Guid ProductId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ProductReviewsDto
{
    // Null when there are no reviews yet.
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }

    // How many reviews gave 5, 4, 3, 2 and 1 stars, in that order.
    public List<StarCountDto> Breakdown { get; set; } = new();

    public PaginatedList<ReviewDto> Reviews { get; set; } = null!;

    public ReviewDto? MyReview { get; set; }

    // True when the customer received this product (so they may review it).
    public bool CanReview { get; set; }
}

public class StarCountDto
{
    public int Stars { get; set; }
    public int Count { get; set; }
}
