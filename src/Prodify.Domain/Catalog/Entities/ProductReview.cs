using Prodify.Domain.Common;

namespace Prodify.Domain.Catalog.Entities;

// A customer's star rating (and optional words) for a product they received.
// One per customer per product; they can edit it. Admins can hide abusive ones.
public class ProductReview : AuditableEntity
{
    public const int MaxTitleLength = 120;
    public const int MaxCommentLength = 2000;

    public Guid ProductId { get; private set; }
    public Guid CustomerId { get; private set; }

    // Shown with the review, e.g. "Tolu A."
    public string ReviewerName { get; private set; } = null!;

    public int Rating { get; private set; }
    public string? Title { get; private set; }
    public string? Comment { get; private set; }

    public bool IsHidden { get; private set; }
    public string? HiddenReason { get; private set; }

    private ProductReview()
    {
    }

    private ProductReview(Guid id, Guid productId, Guid customerId, string reviewerName) : base(id)
    {
        ProductId = productId;
        CustomerId = customerId;
        ReviewerName = reviewerName;
    }

    public static ProductReview Create(Guid productId, Guid customerId, string reviewerName, int rating, string? title, string? comment)
    {
        if (string.IsNullOrWhiteSpace(reviewerName))
            throw new ArgumentException("Reviewer name cannot be empty.", nameof(reviewerName));

        var review = new ProductReview(Guid.NewGuid(), productId, customerId, reviewerName.Trim());
        review.Update(rating, title, comment);
        return review;
    }

    public void Update(int rating, string? title, string? comment)
    {
        if (rating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5 stars.");

        title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

        if (title?.Length > MaxTitleLength)
            throw new ArgumentException($"Title can be at most {MaxTitleLength} characters.", nameof(title));
        if (comment?.Length > MaxCommentLength)
            throw new ArgumentException($"Review can be at most {MaxCommentLength} characters.", nameof(comment));

        Rating = rating;
        Title = title;
        Comment = comment;
    }

    public void Hide(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Give a reason for hiding the review.", nameof(reason));

        IsHidden = true;
        HiddenReason = reason.Trim();
    }

    public void Unhide()
    {
        IsHidden = false;
        HiddenReason = null;
    }
}
