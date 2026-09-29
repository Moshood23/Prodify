using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prodify.Infrastructure.Email;
using Prodify.Infrastructure.Persistence;

namespace Prodify.Infrastructure.BackgroundJobs;

// Every few seconds: sends queued emails that are due. Failures are retried later.
public class EmailDispatchService : BackgroundService
{
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailDispatchService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public EmailDispatchService(IServiceScopeFactory scopeFactory, ILogger<EmailDispatchService> logger)
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
                var context = scope.ServiceProvider.GetRequiredService<ProdifyDbContext>();
                var transport = scope.ServiceProvider.GetRequiredService<IEmailTransport>();

                var now = DateTime.UtcNow;
                var due = await context.EmailMessages
                    .Where(e => e.SentAt == null && e.Attempts < EmailMessage.MaxAttempts && e.NextAttemptAt <= now)
                    .OrderBy(e => e.CreatedAt)
                    .Take(BatchSize)
                    .ToListAsync(stoppingToken);

                foreach (var email in due)
                {
                    try
                    {
                        await transport.SendAsync(email, stoppingToken);
                        email.MarkSent();
                        _logger.LogInformation("Sent email \"{Subject}\" to {To}", email.Subject, email.ToAddress);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        email.MarkFailed(ex.Message);
                        _logger.LogWarning(ex, "Could not send email \"{Subject}\" to {To} (attempt {Attempt})", email.Subject, email.ToAddress, email.Attempts);
                    }
                }

                if (due.Count > 0)
                    await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while sending emails.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
