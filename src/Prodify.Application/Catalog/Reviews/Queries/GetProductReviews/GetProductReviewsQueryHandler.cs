using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Reviews.Common;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Catalog.Reviews.Queries.GetProductReviews;

public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, ProductReviewsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetProductReviewsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ProductReviewsDto> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        if (!await _context.Products.AnyAsync(p => p.Id == request.ProductId, cancellationToken))
            throw new NotFoundException("Product", request.ProductId);

        var visible = _context.ProductReviews.Where(r => r.ProductId == request.ProductId && !r.IsHidden);

        var counts = await visible
            .GroupBy(r => r.Rating)
            .Select(g => new { Stars = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Stars, x => x.Count, cancellationToken);

        var total = counts.Values.Sum();
        var average = total == 0 ? (decimal?)null : Math.Round((decimal)counts.Sum(c => c.Key * c.Value) / total, 1);

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var page = await visible
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDto())
            .ToListAsync(cancellationToken);

        var result = new ProductReviewsDto
        {
            AverageRating = average,
            ReviewCount = total,
            Breakdown = Enumerable.Range(1, 5).Reverse()
                .Select(stars => new StarCountDto { Stars = stars, Count = counts.GetValueOrDefault(stars) })
                .ToList(),
            Reviews = new PaginatedList<ReviewDto>(page, total, pageNumber, pageSize),
        };

        if (_currentUser.CustomerId is { } customerId)
        {
            result.MyReview = await _context.ProductReviews
                .Where(r => r.ProductId == request.ProductId && r.CustomerId == customerId)
                .Select(ToDto())
                .FirstOrDefaultAsync(cancellationToken);

            result.CanReview = await _context.HasReceivedProductAsync(customerId, request.ProductId, cancellationToken);
        }

        return result;
    }

    private static System.Linq.Expressions.Expression<Func<Domain.Catalog.Entities.ProductReview, ReviewDto>> ToDto() =>
        r => new ReviewDto
        {
            Id = r.Id,
            Rating = r.Rating,
            Title = r.Title,
            Comment = r.Comment,
            ReviewerName = r.ReviewerName,
            CreatedAt = r.CreatedAt,
            EditedAt = r.ModifiedAt,
            IsHidden = r.IsHidden,
        };
}
