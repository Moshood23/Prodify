using MediatR;

namespace Prodify.Application.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommand : IRequest
{
    public Guid NotificationId { get; set; }
}