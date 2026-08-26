using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prodify.Application.Common.Interfaces;
using Prodify.Infrastructure.Persistence;
using System.Net;
using System.Net.Mail;

namespace Prodify.Infrastructure.Notifications;

public class EmailNotificationService : INotificationService
{
    private readonly ProdifyDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        ProdifyDbContext dbContext,
        IConfiguration configuration,
        ILogger<EmailNotificationService> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(Guid recipientId, string title, string message, CancellationToken cancellationToken = default)
    {
        var email = await ResolveEmailAsync(recipientId, cancellationToken);

        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Could not resolve email address for recipient {RecipientId}.", recipientId);
            return;
        }

        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPort = _configuration.GetValue<int>("Email:SmtpPort", 587);
        var fromAddress = _configuration["Email:FromAddress"] ?? "noreply@prodify.local";
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPassword = _configuration["Email:SmtpPassword"];

        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            _logger.LogWarning("Email SMTP settings are not configured. Skipping email to {Email}.", email);
            return;
        }

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPassword),
            EnableSsl = true
        };

        using var mailMessage = new MailMessage(fromAddress, email, title, message);

        await client.SendMailAsync(mailMessage, cancellationToken);
    }

    private async Task<string?> ResolveEmailAsync(Guid recipientId, CancellationToken cancellationToken)
    {
        var customerEmail = await _dbContext.Customers
            .Where(c => c.Id == recipientId)
            .Select(c => c.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (customerEmail is not null)
            return customerEmail;

        var sellerEmail = await _dbContext.Sellers
            .Where(s => s.Id == recipientId)
            .Select(s => s.Email)
            .FirstOrDefaultAsync(cancellationToken);

        return sellerEmail;
    }
}