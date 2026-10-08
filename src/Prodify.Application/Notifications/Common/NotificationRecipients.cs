using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Notifications.Common;

public static class NotificationRecipients
{
    // Notifications are addressed to a customer or seller profile, never to a raw user id.
    // A seller who also shops sees both.
    public static List<Guid> Of(ICurrentUserService currentUser) =>
        new[] { currentUser.CustomerId, currentUser.SellerId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
}
