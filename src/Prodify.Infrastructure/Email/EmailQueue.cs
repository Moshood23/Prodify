using Prodify.Application.Common.Interfaces;
using Prodify.Infrastructure.Persistence;

namespace Prodify.Infrastructure.Email;

// Adds the email to the database; it is saved together with the rest of the request's changes.
public class EmailQueue : IEmailQueue
{
    private readonly ProdifyDbContext _context;

    public EmailQueue(ProdifyDbContext context)
    {
        _context = context;
    }

    public void Enqueue(string toAddress, string? toName, EmailContent content) =>
        _context.EmailMessages.Add(new EmailMessage(toAddress.Trim(), toName, content.Subject, content.Html, content.Text));
}
