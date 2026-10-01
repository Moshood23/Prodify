using MediatR;

namespace Prodify.Application.Admin.Reviews.Commands.SetReviewHidden;

// Hides a review from the shop (with a reason), or shows it again.
public class SetReviewHiddenCommand : IRequest
{
    public Guid ReviewId { get; set; }
    public bool Hidden { get; set; }
    public string? Reason { get; set; }
}
