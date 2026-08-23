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
    General
}

public class Notification : AuditableEntity
{
    public Guid RecipientId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification()
    {
    }

    private Notification(Guid id, Guid recipientId, NotificationType type, string title, string message) : base(id)
    {
        RecipientId = recipientId;
        Type = type;
        Title = title;
        Message = message;
        IsRead = false;
    }

    public static Notification Create(Guid recipientId, NotificationType type, string title, string message)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Notification title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Notification message cannot be empty.", nameof(message));

        return new Notification(Guid.NewGuid(), recipientId, type, title.Trim(), message.Trim());
    }

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}