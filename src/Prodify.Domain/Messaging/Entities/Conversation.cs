using System.Text.RegularExpressions;
using Prodify.Domain.Common;

namespace Prodify.Domain.Messaging.Entities;

public enum MessageSender
{
    Customer,
    Seller
}

// A customer talking to a seller about one product, one order, or in general.
// The messages are stored separately (ConversationMessage); this keeps what the inbox shows.
public class Conversation : AuditableEntity
{
    public const int MaxSubjectLength = 200;
    public const int PreviewLength = 140;

    public Guid CustomerId { get; private set; }
    public Guid SellerId { get; private set; }

    // What it is about: a product, an order, or neither.
    public Guid? ProductId { get; private set; }
    public Guid? OrderId { get; private set; }

    // "Golden Penny Spaghetti" or "Order ORD-20261011-1A2B3C4D", kept as it was when the chat started.
    public string Subject { get; private set; } = null!;

    public DateTime LastMessageAt { get; private set; }
    public string LastMessagePreview { get; private set; } = "";
    public MessageSender LastSender { get; private set; }

    // Messages the customer / the seller hasn't read yet.
    public int CustomerUnread { get; private set; }
    public int SellerUnread { get; private set; }

    private Conversation()
    {
    }

    private Conversation(Guid id) : base(id)
    {
    }

    public static Conversation Start(Guid customerId, Guid sellerId, Guid? productId, Guid? orderId, string subject)
    {
        if (customerId == Guid.Empty || sellerId == Guid.Empty)
            throw new ArgumentException("A conversation needs a customer and a seller.");

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("A conversation needs a subject.", nameof(subject));

        var trimmed = subject.Trim();
        return new Conversation(Guid.NewGuid())
        {
            CustomerId = customerId,
            SellerId = sellerId,
            ProductId = productId,
            OrderId = orderId,
            Subject = trimmed.Length > MaxSubjectLength ? trimmed[..MaxSubjectLength] : trimmed
        };
    }

    // Adds a message from one side. Phone numbers and email addresses are hidden, so deals stay on Prodify.
    public ConversationMessage Post(MessageSender from, string body, DateTime now)
    {
        var message = ConversationMessage.Create(Id, from, body, now);

        LastMessageAt = now;
        LastSender = from;
        LastMessagePreview = message.Body.Length > PreviewLength ? message.Body[..PreviewLength] + "..." : message.Body;

        if (from == MessageSender.Customer)
            SellerUnread++;
        else
            CustomerUnread++;

        return message;
    }

    public void MarkReadBy(MessageSender reader)
    {
        if (reader == MessageSender.Customer)
            CustomerUnread = 0;
        else
            SellerUnread = 0;
    }

    public int UnreadFor(MessageSender reader) => reader == MessageSender.Customer ? CustomerUnread : SellerUnread;
}

public class ConversationMessage : AuditableEntity
{
    public const int MaxLength = 2000;

    public Guid ConversationId { get; private set; }
    public MessageSender Sender { get; private set; }
    public string Body { get; private set; } = null!;
    public DateTime SentAt { get; private set; }

    // A phone number or email address was taken out.
    public bool HadContactDetails { get; private set; }

    private ConversationMessage()
    {
    }

    private ConversationMessage(Guid id) : base(id)
    {
    }

    internal static ConversationMessage Create(Guid conversationId, MessageSender sender, string body, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Write a message.", nameof(body));

        var trimmed = body.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"Keep messages under {MaxLength} characters.", nameof(body));

        var (text, masked) = ContactDetails.Hide(trimmed);
        return new ConversationMessage(Guid.NewGuid())
        {
            ConversationId = conversationId,
            Sender = sender,
            Body = text,
            SentAt = now,
            HadContactDetails = masked
        };
    }
}

// Finds phone numbers and email addresses in a message and replaces them with [hidden].
public static class ContactDetails
{
    public const string Replacement = "[hidden]";

    // Also spelled out: "tolu at gmail dot com", "tolu (at) gmail (dot) com".
    private static readonly Regex Email = new(
        @"[A-Za-z0-9._%+-]+(\s*(@|\(at\)|\[at\])\s*|\s+at\s+)[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*(\s*(\.|\(dot\)|\[dot\])\s*|\s+dot\s+)[A-Za-z]{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // 10 to 15 digits, maybe split by spaces, dashes, dots or brackets, maybe starting with +.
    // Not inside a longer code (like an order number), which is why letters, digits and dashes can't touch it.
    private static readonly Regex Phone = new(
        @"(?<![A-Za-z0-9-])\+?\(?\d[\d\s().-]{8,20}\d(?![A-Za-z0-9])",
        RegexOptions.Compiled);

    public static (string Text, bool Hidden) Hide(string text)
    {
        var hidden = false;

        var result = Email.Replace(text, _ =>
        {
            hidden = true;
            return Replacement;
        });

        result = Phone.Replace(result, m =>
        {
            var digits = m.Value.Count(char.IsDigit);
            if (digits < 10 || digits > 15)
                return m.Value;

            hidden = true;
            return Replacement;
        });

        return (result, hidden);
    }
}
