using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Payments.Entities;
using Prodify.Infrastructure.Persistence;

namespace Prodify.Infrastructure.BackgroundJobs;

public class PaymentRetryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentRetryService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(2);
    private const int MaxRetryAttempts = 3;

    public PaymentRetryService(
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentRetryService> logger)
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
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                var stuckPayments = await dbContext.Payments
                    .Where(p => p.Status == PaymentStatus.Failed || p.Status == PaymentStatus.Processing)
                    .ToListAsync(stoppingToken);

                foreach (var payment in stuckPayments)
                {
                    if (payment.Attempts.Count >= MaxRetryAttempts)
                        continue;

                    var attempt = payment.StartAttempt();
                    var result = await paymentService.ChargeAsync(payment.Amount, "retry-token", stoppingToken);

                    if (result.Succeeded)
                        payment.CompleteAttempt(attempt.Id, result.GatewayReference!);
                    else
                        payment.FailAttempt(attempt.Id, result.FailureReason);
                }

                if (stuckPayments.Any())
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Retried {Count} stuck payments.", stuckPayments.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrying stuck payments.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}