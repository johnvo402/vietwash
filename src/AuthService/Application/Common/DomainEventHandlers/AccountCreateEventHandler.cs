using Application.Common.Interfaces.Services.DistributedCache;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Events;
using Domain.Aggregates.PubSubLogs;
using Mediator;
using Serilog;

namespace Application.Common.DomainEventHandlers;

public class AccountCreateEventHandler(ILogger logger, IPubSubFactory queueFactory)
{
    public async ValueTask Handle(
        AccountCreateEvent notification,
        CancellationToken cancellationToken
    )
    {
        logger.Information("AccountCreateEventHandler: {@Id}", notification.AccountId);
        CreateAccountEvent mappingUser = new CreateAccountEvent();
        mappingUser.MappingFrom(notification);

        var check = await queueFactory
            .GetPubSub(PubSubType.Origin)
            .PublishAsync(mappingUser, "CreateAccountEvent");
        if (!check)
        {
            logger.Error("UserCreateEventHandler: {@User} enqueue failed", notification.AccountId);
        }

        await Task.CompletedTask;
    }
}
