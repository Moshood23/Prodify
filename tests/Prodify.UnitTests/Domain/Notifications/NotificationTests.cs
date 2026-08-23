using Prodify.Domain.Notifications.Entities;

namespace Prodify.UnitTests.Domain.Notifications;

public class NotificationTests
{
    [Fact]
    public void Create_WithValidData_CreatesUnreadNotification()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationType.OrderPlaced, "Order Placed", "Your order has been placed.");

        Assert.False(notification.IsRead);
        Assert.Null(notification.ReadAt);
    }

    [Fact]
    public void Create_WithEmptyTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Notification.Create(Guid.NewGuid(), NotificationType.General, string.Empty, "Message"));
    }

    [Fact]
    public void MarkAsRead_SetsIsReadTrueAndReadAt()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationType.OrderPlaced, "Order Placed", "Your order has been placed.");
        notification.MarkAsRead();

        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
    }

    [Fact]
    public void MarkAsRead_WhenAlreadyRead_DoesNotThrow()
    {
        var notification = Notification.Create(Guid.NewGuid(), NotificationType.OrderPlaced, "Order Placed", "Your order has been placed.");
        notification.MarkAsRead();
        var firstReadAt = notification.ReadAt;

        notification.MarkAsRead();

        Assert.Equal(firstReadAt, notification.ReadAt);
    }
}