using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prodify.Domain.Inventory.Entities;
using Prodify.Infrastructure.Persistence;

namespace Prodify.Infrastructure.BackgroundJobs;

public class ReservationExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationExpirationService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public ReservationExpirationService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReservationExpirationService> logger)
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
                var dbContext = scope.ServiceProvider.GetRequiredService<ProdifyDbContext>();

                var now = DateTime.UtcNow;

                var itemsWithExpiredReservations = await dbContext.InventoryItems
                    .Where(i => i.Reservations.Any(r => r.Status == ReservationStatus.Active && r.ExpiresAt <= now))
                    .ToListAsync(stoppingToken);

                foreach (var item in itemsWithExpiredReservations)
                {
                    var expiredReservations = item.Reservations
                        .Where(r => r.Status == ReservationStatus.Active && r.ExpiresAt <= now)
                        .ToList();

                    foreach (var reservation in expiredReservations)
                    {
                        item.ExpireReservation(reservation.Id);
                    }
                }

                if (itemsWithExpiredReservations.Any())
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Expired reservations processed for {Count} inventory items.", itemsWithExpiredReservations.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while expiring stock reservations.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}