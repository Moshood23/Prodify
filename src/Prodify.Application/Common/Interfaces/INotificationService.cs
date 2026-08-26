namespace Prodify.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAsync(Guid recipientId, string title, string message, CancellationToken cancellationToken = default);
}