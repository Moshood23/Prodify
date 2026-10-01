using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Admin.Reviews.Commands.SetReviewHidden;

public class SetReviewHiddenCommandHandler : IRequestHandler<SetReviewHiddenCommand>
{
    private readonly IApplicationDbContext _context;

    public SetReviewHiddenCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SetReviewHiddenCommand request, CancellationToken cancellationToken)
    {
        var review = await _context.ProductReviews.FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken)
            ?? throw new NotFoundException("Review", request.ReviewId);

        if (request.Hidden)
            review.Hide(request.Reason!);
        else
            review.Unhide();
    }
}
