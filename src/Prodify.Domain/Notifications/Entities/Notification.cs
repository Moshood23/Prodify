using Prodify.Domain.Common;

namespace Prodify.Domain.Notifications.Entities;

public enum NotificationType
{
    OrderPlaced,
    OrderShipped,
    OrderDelivered,
    OrderCancelled,
    PaymentSuccessful,
    PaymentFailed,
    General,
    // For sellers:
    NewOrder,
    AccountUpdate,
    Payout
}

public class Notification : AuditableEntity
{
    public Guid RecipientId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    // Website page the notification opens, e.g. /orders/{id}.
    public string? Link { get; private set; }

    private Notification()
    {
    }

    private Notification(Guid id, Guid recipientId, NotificationType type, string title, string message, string? link) : base(id)
    {
        RecipientId = recipientId;
        Type = type;
        Title = title;
        Message = message;
        Link = link;
        IsRead = false;
    }

    public static Notification Create(Guid recipientId, NotificationType type, string title, string message, string? link = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Notification title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Notification message cannot be empty.", nameof(message));

        if (link is not null && !link.StartsWith('/'))
            throw new ArgumentException("Notification link must be a page on the website, e.g. /orders/123.", nameof(link));

        return new Notification(Guid.NewGuid(), recipientId, type, title.Trim(), message.Trim(), link);
    }

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
