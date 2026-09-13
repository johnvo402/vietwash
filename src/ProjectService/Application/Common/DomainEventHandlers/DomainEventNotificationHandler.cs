using Contracts.Application.Common.Events;
using Domain.Aggregates.Branches.Events;
using Mediator;

namespace Application.Common.DomainEventHandlers;

public sealed class DomainEventNotificationHandler(BranchCreateEventHandler branchCreateHandler)
    : INotificationHandler<DomainEventNotification>
{
    public ValueTask Handle(
        DomainEventNotification notification,
        CancellationToken cancellationToken
    ) =>
        notification.DomainEvent switch
        {
            BranchCreateEvent domainEvent => branchCreateHandler.Handle(
                domainEvent,
                cancellationToken
            ),
            _ => ValueTask.CompletedTask,
        };
}
