using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Common;

namespace Prodify.Infrastructure.Messaging.InProcess;

public class MediatRMessagePublisher : IMessagePublisher
{
    private readonly IMediator _mediator;

    public MediatRMessagePublisher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        var notification = Activator.CreateInstance(notificationType, domainEvent);

        if (notification is INotification typedNotification)
        {
            await _mediator.Publish(typedNotification, cancellationToken);
        }
    }
}