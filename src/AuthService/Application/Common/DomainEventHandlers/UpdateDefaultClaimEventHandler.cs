using Application.Common.Interfaces.Services.Identity;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Events;
using Mediator;

namespace Application.Common.DomainEventHandlers;

public class UpdateDefaultClaimEventHandler
    : INotificationHandler<UpdateDefaultUserClaimEvent>
{
    public async ValueTask Handle(
        UpdateDefaultUserClaimEvent notification,
        CancellationToken cancellationToken
    )
    {
        await Task.CompletedTask;
    }
}
