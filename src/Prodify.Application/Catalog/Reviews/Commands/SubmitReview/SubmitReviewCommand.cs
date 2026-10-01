using MediatR;

namespace Prodify.Application.Catalog.Reviews.Commands.SubmitReview;

// Writes the customer's review of a product, or updates it if they already wrote one.
public class SubmitReviewCommand : IRequest<Guid>
{
    public Guid ProductId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
}
