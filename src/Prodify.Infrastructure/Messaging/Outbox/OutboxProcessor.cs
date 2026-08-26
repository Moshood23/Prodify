using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Common;
using Prodify.Infrastructure.Persistence;
using System.Text.Json;

namespace Prodify.Infrastructure.Messaging.Outbox;

public class OutboxProcessor
{
    private readonly ProdifyDbContext _dbContext;
    private readonly IMessagePublisher _messagePublisher;

    public OutboxProcessor(ProdifyDbContext dbContext, IMessagePublisher messagePublisher)
    {
        _dbContext = dbContext;
        _messagePublisher = messagePublisher;
    }

    public async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        var pendingMessages = await _dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var message in pendingMessages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);

                if (eventType is null)
                {
                    message.MarkFailed($"Could not resolve type '{message.Type}'.");
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType) as DomainEvent;

                if (domainEvent is null)
                {
                    message.MarkFailed("Failed to deserialize domain event payload.");
                    continue;
                }

                await _messagePublisher.PublishAsync(domainEvent, cancellationToken);
                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.Message);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}