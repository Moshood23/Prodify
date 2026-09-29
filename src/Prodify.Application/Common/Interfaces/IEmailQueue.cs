namespace Prodify.Application.Common.Interfaces;

public record EmailContent(string Subject, string Html, string Text);

// Emails are queued in the database as part of the same save as the action
// that triggered them (so nothing is sent for an action that failed) and a
// background job sends them.
public interface IEmailQueue
{
    void Enqueue(string toAddress, string? toName, EmailContent content);
}
