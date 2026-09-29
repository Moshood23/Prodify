namespace Prodify.Infrastructure.Email;

// One queued email. Sent by EmailDispatchService, retried with growing gaps if sending fails.
public class EmailMessage
{
    public const int MaxAttempts = 5;

    public Guid Id { get; private set; }
    public string ToAddress { get; private set; } = null!;
    public string? ToName { get; private set; }
    public string Subject { get; private set; } = null!;
    public string HtmlBody { get; private set; } = null!;
    public string TextBody { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime NextAttemptAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    private EmailMessage()
    {
    }

    public EmailMessage(string toAddress, string? toName, string subject, string htmlBody, string textBody)
    {
        Id = Guid.NewGuid();
        ToAddress = toAddress;
        ToName = toName;
        Subject = subject;
        HtmlBody = htmlBody;
        TextBody = textBody;
        CreatedAt = DateTime.UtcNow;
        NextAttemptAt = CreatedAt;
    }

    public void MarkSent()
    {
        SentAt = DateTime.UtcNow;
        LastError = null;
    }

    // Waits 1, 2, 4, 8 minutes between tries, then gives up.
    public void MarkFailed(string error)
    {
        Attempts++;
        LastError = error.Length > 1000 ? error[..1000] : error;
        NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, Attempts - 1));
    }
}
