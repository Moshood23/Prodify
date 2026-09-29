namespace Prodify.Infrastructure.Email;

// Actually delivers an email (to a folder or an SMTP server). Throws if it can't.
public interface IEmailTransport
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
