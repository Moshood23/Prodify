using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prodify.Infrastructure.Messaging.Outbox;

namespace Prodify.Infrastructure.BackgroundJobs;

public class OutboxProcessorHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public OutboxProcessorHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessorHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

                await processor.ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}