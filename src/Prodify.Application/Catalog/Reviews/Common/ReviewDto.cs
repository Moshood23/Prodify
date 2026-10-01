namespace Prodify.Application.Catalog.Reviews.Common;

public class ReviewDto
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string ReviewerName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }

    // Only shown to the reviewer themselves (on their own review).
    public bool IsHidden { get; set; }
}
