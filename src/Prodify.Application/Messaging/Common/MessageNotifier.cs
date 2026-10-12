using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Messaging.Entities;
using Prodify.Domain.Notifications.Entities;

namespace Prodify.Application.Messaging.Common;

public static class MessageNotifier
{
    // A bell notification for the other side, only for the first unread message, so a chat doesn't flood the bell.
    public static void NotifyIfFirstUnread(this IApplicationDbContext context, Conversation conversation, MessageSender from, string senderName)
    {
        var to = from == MessageSender.Customer ? MessageSender.Seller : MessageSender.Customer;
        if (conversation.UnreadFor(to) != 1)
            return;

        var (recipientId, link) = to == MessageSender.Customer
            ? (conversation.CustomerId, $"/account/messages/{conversation.Id}")
            : (conversation.SellerId, $"/seller/messages/{conversation.Id}");

        context.Add(Notification.Create(recipientId, NotificationType.Message, $"New message from {senderName}",
            $"About {conversation.Subject}: {conversation.LastMessagePreview}", link));
    }
}
