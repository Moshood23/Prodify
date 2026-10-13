using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prodify.Application.Inventory.LowStock;

namespace Prodify.Infrastructure.BackgroundJobs;

// Every 5 minutes: tells sellers when products run low, and sends the daily low stock email.
public class LowStockAlertService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LowStockAlertService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public LowStockAlertService(IServiceScopeFactory scopeFactory, ILogger<LowStockAlertService> logger)
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

                var result = await sender.Send(new CheckLowStockCommand(), stoppingToken);

                if (result.SellersNotified > 0 || result.EmailsSent > 0)
                    _logger.LogInformation(
                        "Low stock: notified {Sellers} sellers, sent {Emails} daily emails.", result.SellersNotified, result.EmailsSent);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while checking for low stock.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
