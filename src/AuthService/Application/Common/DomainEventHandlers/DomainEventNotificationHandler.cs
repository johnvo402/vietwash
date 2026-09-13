using Contracts.Application.Common.Events;
using Domain.Aggregates.Accounts.Events;
using Mediator;

namespace Application.Common.DomainEventHandlers;

public sealed class DomainEventNotificationHandler(AccountCreateEventHandler accountCreateHandler)
    : INotificationHandler<DomainEventNotification>
{
    public ValueTask Handle(
        DomainEventNotification notification,
        CancellationToken cancellationToken
    ) =>
        notification.DomainEvent switch
        {
            AccountCreateEvent domainEvent => accountCreateHandler.Handle(
                domainEvent,
                cancellationToken
            ),
            _ => ValueTask.CompletedTask,
        };
}
