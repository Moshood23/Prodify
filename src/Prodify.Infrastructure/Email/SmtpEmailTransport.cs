using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Prodify.Infrastructure.Email;

// Production: sends through an SMTP server (Brevo, Mailgun, ...), set in the "Email" settings.
public class SmtpEmailTransport : IEmailTransport
{
    private readonly EmailSettings _settings;

    public SmtpEmailTransport(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
            throw new InvalidOperationException("Email:SmtpHost is not set.");

        using var mail = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = message.Subject,
            Body = message.TextBody,
        };
        mail.To.Add(new MailAddress(message.ToAddress, message.ToName ?? string.Empty));
        mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.HtmlBody, null, "text/html"));

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.SmtpEnableSsl,
            Credentials = string.IsNullOrWhiteSpace(_settings.SmtpUser)
                ? null
                : new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword),
        };

        await client.SendMailAsync(mail, cancellationToken);
    }
}
