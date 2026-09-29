namespace Prodify.Infrastructure.Email;

// The "Email" section of appsettings.
public class EmailSettings
{
    // "Files": save each email as an .html file in PickupDirectory (development).
    // "Smtp": send through the SMTP server below (e.g. Brevo, Mailgun, Gmail).
    public string Mode { get; set; } = "Files";

    public string FromAddress { get; set; } = "noreply@prodify.local";
    public string FromName { get; set; } = "Prodify";

    // Relative to the API project, like the uploads folder.
    public string PickupDirectory { get; set; } = "emails";

    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public bool SmtpEnableSsl { get; set; } = true;
}
