using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Notifications.Common;

namespace Prodify.Application.Notifications.Queries.GetUnreadCount;

// The number on the bell. Small and cheap, because the website asks for it every minute.
public class GetUnreadCountQuery : IRequest<int>
{
}

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadCountQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var recipientIds = NotificationRecipients.Of(_currentUser);

        return _context.Notifications
            .CountAsync(n => recipientIds.Contains(n.RecipientId) && !n.IsRead, cancellationToken);
    }
}
