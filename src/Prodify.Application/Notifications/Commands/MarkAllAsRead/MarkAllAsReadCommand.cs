using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Notifications.Common;

namespace Prodify.Application.Notifications.Commands.MarkAllAsRead;

public class MarkAllAsReadCommand : IRequest
{
}

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkAllAsReadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var recipientIds = NotificationRecipients.Of(_currentUser);

        var unread = await _context.Notifications
            .Where(n => recipientIds.Contains(n.RecipientId) && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unread)
            notification.MarkAsRead();
    }
}
