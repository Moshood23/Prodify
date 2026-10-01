using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Reviews.Common;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.Application.Catalog.Reviews.Commands.SubmitReview;

public class SubmitReviewCommandHandler : IRequestHandler<SubmitReviewCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SubmitReviewCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        if (!await _context.Products.AnyAsync(p => p.Id == request.ProductId, cancellationToken))
            throw new NotFoundException("Product", request.ProductId);

        if (!await _context.HasReceivedProductAsync(customerId, request.ProductId, cancellationToken))
            throw new BusinessRuleException("You can review this product once an order with it has been delivered to you.");

        var review = await _context.ProductReviews
            .FirstOrDefaultAsync(r => r.ProductId == request.ProductId && r.CustomerId == customerId, cancellationToken);

        if (review is not null)
        {
            review.Update(request.Rating, request.Title, request.Comment);
            return review.Id;
        }

        var customer = await _context.Customers
            .Where(c => c.Id == customerId)
            .Select(c => new { c.FirstName, c.LastName })
            .FirstAsync(cancellationToken);

        review = ProductReview.Create(
            request.ProductId, customerId, ReviewRules.ShortName(customer.FirstName, customer.LastName),
            request.Rating, request.Title, request.Comment);
        _context.Add(review);

        return review.Id;
    }
}
