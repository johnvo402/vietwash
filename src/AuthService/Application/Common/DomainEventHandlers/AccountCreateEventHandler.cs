using Application.Common.Interfaces.Services.DistributedCache;
using AutoMapper;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Events;
using Domain.Aggregates.QueueLogs;
using Mediator;
using Microsoft.AspNetCore.Http;
using Serilog;
using static Application.Common.DomainEventHandlers.AccountCreateEventHandler;

namespace Application.Common.DomainEventHandlers;

public class AccountCreateEventHandler(ILogger logger, IQueueFactory queueFactory, IMapper mapper)
    : INotificationHandler<AccountCreateEvent>
{
    public async ValueTask Handle(
        AccountCreateEvent notification,
        CancellationToken cancellationToken
    )
    {
        logger.Information("AccountCreateEventHandler: {@Id}", notification.Account.Id);
        CreateAccountEvent mappingUser = mapper.Map<CreateAccountEvent>(notification.Account);

        var check = await queueFactory.GetQueue(QueueType.OriginQueue).EnqueueAsync(mappingUser);
        if (!check)
        {
            logger.Error("UserCreateEventHandler: {@User} enqueue failed", notification.Account.Id);
        }

        await Task.CompletedTask;
    }

    public class CreateAccountEvent
    {
        public long Id { get; set; }
        public string? Password { get; set; }
        public Gender? Gender { get; set; }
        public AccountStatus Status { get; set; }
        public string Role { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly? BirthDay { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
