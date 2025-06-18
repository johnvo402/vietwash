using Application.Common.Interfaces.Services.DistributedCache;
using AutoMapper;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Events;
using Domain.Aggregates.PubSubLogs;
using Mediator;
using Serilog;

namespace Application.Common.DomainEventHandlers;

public class AccountCreateEventHandler(ILogger logger, IPubSubFactory queueFactory, IMapper mapper)
    : INotificationHandler<AccountCreateEvent>
{
    public async ValueTask Handle(
        AccountCreateEvent notification,
        CancellationToken cancellationToken
    )
    {
        logger.Information("AccountCreateEventHandler: {@Id}", notification.Account.Id);
        CreateAccountEvent mappingUser = mapper.Map<CreateAccountEvent>(notification.Account);

        var check = await queueFactory.GetPubSub(PubSubType.Origin).PublishAsync(mappingUser);
        if (!check)
        {
            logger.Error("UserCreateEventHandler: {@User} enqueue failed", notification.Account.Id);
        }

        await Task.CompletedTask;
    }

    public class CreateAccountEvent
    {
        public long Id { get; set; }
        public Ulid PublicId { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly BirthDay { get; set; }
        public Gender? Gender { get; set; }
        public string? AvtUrl { get; set; }
        public string? Role { get; set; }
        public bool Disabled { get; set; }
        public CustomerGroup? CustomerGroup { get; set; }

        public AccountStatus Status { get; set; }
    }
}
