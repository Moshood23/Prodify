using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Messaging.Entities;

namespace Prodify.Application.Messaging.Common;

public static class ConversationAccess
{
    // The conversations the signed-in customer (or seller) is part of.
    public static IQueryable<Conversation> OwnedBy(this IQueryable<Conversation> conversations, ICurrentUserService currentUser, MessageSender who)
    {
        if (who == MessageSender.Customer)
        {
            var customerId = currentUser.GetRequiredCustomerId();
            return conversations.Where(c => c.CustomerId == customerId);
        }

        var sellerId = currentUser.GetRequiredSellerId();
        return conversations.Where(c => c.SellerId == sellerId);
    }

    // One of their conversations, or 404 (also for someone else's, so ids can't be probed).
    public static async Task<Conversation> GetOwnConversationAsync(
        this IApplicationDbContext context, ICurrentUserService currentUser, MessageSender who, Guid id, CancellationToken cancellationToken) =>
        await context.Conversations.OwnedBy(currentUser, who).FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Conversation), id);
}
