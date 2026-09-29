using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Prodify.Infrastructure.Email;

// Development: every email becomes an .html file you can open in a browser.
public class FileEmailTransport : IEmailTransport
{
    private readonly string _directory;

    public FileEmailTransport(IOptions<EmailSettings> settings, IHostEnvironment environment)
    {
        _directory = Path.GetFullPath(Path.Combine(environment.ContentRootPath, settings.Value.PickupDirectory));
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_directory);

        var slug = new string(message.Subject.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');
        if (slug.Length > 50)
            slug = slug[..50];
        var fileName = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{slug}-{message.Id.ToString()[..8]}.html";

        // A strip at the top shows who it was for, like an email app would.
        var header = $"<div style=\"font-family:monospace;font-size:13px;background:#fef3c7;padding:8px 12px\">" +
                     $"To: {WebUtility.HtmlEncode(message.ToName ?? "")} &lt;{WebUtility.HtmlEncode(message.ToAddress)}&gt;<br>" +
                     $"Subject: {WebUtility.HtmlEncode(message.Subject)}</div>";

        await File.WriteAllTextAsync(Path.Combine(_directory, fileName), header + message.HtmlBody, cancellationToken);
    }
}
