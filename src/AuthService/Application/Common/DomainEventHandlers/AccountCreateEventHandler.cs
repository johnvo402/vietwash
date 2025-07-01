using Application.Common.Interfaces.Services.DistributedCache;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Events;
using Domain.Aggregates.PubSubLogs;
using Mediator;
using Serilog;

namespace Application.Common.DomainEventHandlers;

public class AccountCreateEventHandler(ILogger logger, IPubSubFactory queueFactory)
    : INotificationHandler<AccountCreateEvent>
{
    public async ValueTask Handle(
        AccountCreateEvent notification,
        CancellationToken cancellationToken
    )
    {
        logger.Information("AccountCreateEventHandler: {@Id}", notification.Account.Id);
        CreateAccountEvent mappingUser = new CreateAccountEvent();
        mappingUser.MappingFrom(notification.Account);

        var check = await queueFactory
            .GetPubSub(PubSubType.Origin)
            .PublishAsync(mappingUser, "CreateAccountEvent");
        if (!check)
        {
            logger.Error("UserCreateEventHandler: {@User} enqueue failed", notification.Account.Id);
        }

        await Task.CompletedTask;
    }
}
