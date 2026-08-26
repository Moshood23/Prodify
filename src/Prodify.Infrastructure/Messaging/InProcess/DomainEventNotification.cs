using MediatR;
using Prodify.Domain.Common;

namespace Prodify.Infrastructure.Messaging.InProcess;

public class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : DomainEvent
{
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}