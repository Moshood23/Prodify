using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Admin.Reviews.Queries.ListAdminReviews;

public class ListAdminReviewsQueryHandler : IRequestHandler<ListAdminReviewsQuery, PaginatedList<AdminReviewDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AdminReviewDto>> Handle(ListAdminReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = _context.ProductReviews
            .Join(_context.Products, r => r.ProductId, p => p.Id, (r, p) => new { Review = r, ProductName = p.Name });

        if (request.Rating.HasValue)
            reviews = reviews.Where(x => x.Review.Rating == request.Rating.Value);

        if (request.Status == "hidden")
            reviews = reviews.Where(x => x.Review.IsHidden);
        else if (request.Status == "visible")
            reviews = reviews.Where(x => !x.Review.IsHidden);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            reviews = reviews.Where(x =>
                x.ProductName.Contains(term)
                || x.Review.ReviewerName.Contains(term)
                || (x.Review.Title != null && x.Review.Title.Contains(term))
                || (x.Review.Comment != null && x.Review.Comment.Contains(term)));
        }

        var total = await reviews.CountAsync(cancellationToken);

        var items = await reviews
            .OrderByDescending(x => x.Review.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AdminReviewDto
            {
                Id = x.Review.Id,
                ProductId = x.Review.ProductId,
                ProductName = x.ProductName,
                CustomerId = x.Review.CustomerId,
                ReviewerName = x.Review.ReviewerName,
                Rating = x.Review.Rating,
                Title = x.Review.Title,
                Comment = x.Review.Comment,
                CreatedAt = x.Review.CreatedAt,
                IsHidden = x.Review.IsHidden,
                HiddenReason = x.Review.HiddenReason,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<AdminReviewDto>(items, total, request.PageNumber, request.PageSize);
    }
}
