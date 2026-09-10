using MediatR;
using Prodify.Application.Common.Models;

namespace Prodify.Application.Notifications.Queries.GetNotifications;

public class GetNotificationsQuery : IRequest<PaginatedList<NotificationDto>>
{
    public Guid RecipientId { get; set; }
    public bool? UnreadOnly { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}