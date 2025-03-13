using Application.Common.Interfaces.Services.DistributedCache;
using Application.Common.Interfaces.Services.Identity;
using Application.Features.Users.Commands.Create;
using AutoMapper;
using Domain.Aggregates.QueueLogs;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Events;
using Mediator;
using Serilog;

namespace Application.Common.DomainEventHandlers;

public class UserCreateEventHandler(ILogger logger, IQueueFactory queueFactory, IMapper mapper)
    : INotificationHandler<UserCreateEvent>
{
    public async ValueTask Handle(
        UserCreateEvent notification,
        CancellationToken cancellationToken
    )
    {
        logger.Information("UserCreateEventHandler: {@UserName}", notification.User.LastName);
        CreateUserCommand mappingUser = mapper.Map<CreateUserCommand>(notification.User);

       var check = await queueFactory.GetQueue(QueueType.OriginQueue).EnqueueAsync(mappingUser);
        if (!check)
        {
            logger.Error("UserCreateEventHandler: {@User} enqueue failed", notification.User.Id);
        }

        await Task.CompletedTask;
    }
}
