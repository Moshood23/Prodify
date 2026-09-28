using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prodify.Application.Ordering.Commands.CancelUnpaidOrders;

namespace Prodify.Infrastructure.BackgroundJobs;

// Once a minute: cancels card orders that weren't paid in time and puts their stock back.
public class UnpaidOrderCancellationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UnpaidOrderCancellationService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public UnpaidOrderCancellationService(IServiceScopeFactory scopeFactory, ILogger<UnpaidOrderCancellationService> logger)
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
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var cancelled = await sender.Send(new CancelUnpaidOrdersCommand(), stoppingToken);

                if (cancelled > 0)
                    _logger.LogInformation("Cancelled {Count} unpaid card orders.", cancelled);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while cancelling unpaid orders.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}