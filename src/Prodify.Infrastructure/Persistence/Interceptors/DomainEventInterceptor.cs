using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Prodify.Domain.Common;
using Prodify.Infrastructure.Messaging.Outbox;

namespace Prodify.Infrastructure.Persistence.Interceptors;

public class DomainEventInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void AddOutboxMessages(DbContext? context)
    {
        if (context is null)
            return;

        var aggregatesWithEvents = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Any())
            .ToList();

        var allDomainEvents = aggregatesWithEvents
            .SelectMany(a => a.DomainEvents)
            .ToList();

        if (!allDomainEvents.Any())
            return;

        var outboxMessages = OutboxPublisher.CreateOutboxMessages(allDomainEvents);
        context.Set<OutboxMessage>().AddRange(outboxMessages);

        foreach (var aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }
    }
}