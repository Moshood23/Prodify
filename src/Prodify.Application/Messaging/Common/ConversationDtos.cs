using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Messaging.Entities;

namespace Prodify.Application.Messaging.Common;

// One conversation in an inbox.
public class ConversationSummaryDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public Guid? ProductId { get; set; }
    public Guid? OrderId { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime LastMessageAt { get; set; }
    public string LastMessagePreview { get; set; } = null!;
    // "Customer" or "Seller".
    public string LastSender { get; set; } = null!;
    // Messages the person looking hasn't read yet (always 0 for Prodify's staff).
    public int Unread { get; set; }
}

// A conversation with its messages, oldest first.
public class ConversationDto : ConversationSummaryDto
{
    public List<MessageDto> Messages { get; set; } = new();
}

public class MessageDto
{
    public Guid Id { get; set; }
    // "Customer" or "Seller".
    public string Sender { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public bool HadContactDetails { get; set; }
}

public static class ConversationQueries
{
    public const int InboxLimit = 100;

    // reader: whose unread count to show (null for Prodify's staff).
    public static Task<List<ConversationSummaryDto>> ToSummariesAsync(
        this IApplicationDbContext context, IQueryable<Conversation> conversations, MessageSender? reader, CancellationToken cancellationToken) =>
        conversations
            .OrderByDescending(c => c.LastMessageAt)
            .Take(InboxLimit)
            .Select(c => new ConversationSummaryDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                CustomerName = context.Customers.Where(u => u.Id == c.CustomerId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault() ?? "",
                SellerId = c.SellerId,
                SellerName = context.Sellers.Where(s => s.Id == c.SellerId).Select(s => s.BusinessName).FirstOrDefault() ?? "",
                Subject = c.Subject,
                ProductId = c.ProductId,
                OrderId = c.OrderId,
                ImageUrl = context.Products
                    .Where(p => p.Id == c.ProductId)
                    .SelectMany(p => p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url))
                    .FirstOrDefault(),
                LastMessageAt = c.LastMessageAt,
                LastMessagePreview = c.LastMessagePreview,
                LastSender = c.LastSender.ToString(),
                Unread = reader == MessageSender.Customer ? c.CustomerUnread : reader == MessageSender.Seller ? c.SellerUnread : 0
            })
            .ToListAsync(cancellationToken);

    public static async Task<ConversationDto?> GetConversationDtoAsync(
        this IApplicationDbContext context, IQueryable<Conversation> conversation, MessageSender? reader, CancellationToken cancellationToken)
    {
        var summary = (await context.ToSummariesAsync(conversation, reader, cancellationToken)).FirstOrDefault();
        if (summary is null)
            return null;

        var messages = await context.ConversationMessages
            .Where(m => m.ConversationId == summary.Id)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Sender = m.Sender.ToString(),
                Body = m.Body,
                SentAt = m.SentAt,
                HadContactDetails = m.HadContactDetails
            })
            .ToListAsync(cancellationToken);

        return new ConversationDto
        {
            Id = summary.Id,
            CustomerId = summary.CustomerId,
            CustomerName = summary.CustomerName,
            SellerId = summary.SellerId,
            SellerName = summary.SellerName,
            Subject = summary.Subject,
            ProductId = summary.ProductId,
            OrderId = summary.OrderId,
            ImageUrl = summary.ImageUrl,
            LastMessageAt = summary.LastMessageAt,
            LastMessagePreview = summary.LastMessagePreview,
            LastSender = summary.LastSender,
            Unread = summary.Unread,
            Messages = messages
        };
    }
}
