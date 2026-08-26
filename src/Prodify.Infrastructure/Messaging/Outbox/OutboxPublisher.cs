using Prodify.Domain.Common;
using System.Text.Json;

namespace Prodify.Infrastructure.Messaging.Outbox;

public static class OutboxPublisher
{
    public static IEnumerable<OutboxMessage> CreateOutboxMessages(IEnumerable<DomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            var eventType = domainEvent.GetType();
            var payload = JsonSerializer.Serialize(domainEvent, eventType);

            yield return new OutboxMessage(eventType.AssemblyQualifiedName!, payload);
        }
    }
}