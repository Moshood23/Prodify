using Prodify.Domain.Common;

namespace Prodify.Application.Common.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
}