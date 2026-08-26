using Microsoft.Extensions.Logging;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Infrastructure.Notifications;

public class LogNotificationService : INotificationService
{
    private readonly ILogger<LogNotificationService> _logger;

    public LogNotificationService(ILogger<LogNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(Guid recipientId, string title, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Notification for {RecipientId}: {Title} - {Message}",
            recipientId, title, message);

        return Task.CompletedTask;
    }
}