using Contracts.Application.Common.Events;
using Mediator;

namespace Application.Common.DomainEventHandlers;

public sealed class DomainEventNotificationHandler
    : INotificationHandler<DomainEventNotification>
{
    public ValueTask Handle(
        DomainEventNotification notification,
        CancellationToken cancellationToken
    ) => ValueTask.CompletedTask;
}
