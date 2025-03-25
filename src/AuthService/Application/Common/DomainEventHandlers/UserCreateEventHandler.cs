using Application.Common.Interfaces.Services.DistributedCache;
using Application.Common.Interfaces.Services.Identity;
using Application.Features.Users.Commands.Create;
using AutoMapper;
using Domain.Aggregates.QueueLogs;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Events;
using Mediator;
using Microsoft.AspNetCore.Http;
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
        CreateUserEvent mappingUser = mapper.Map<CreateUserEvent>(notification.User);

       var check = await queueFactory.GetQueue(QueueType.OriginQueue).EnqueueAsync(mappingUser);
        if (!check)
        {
            logger.Error("UserCreateEventHandler: {@User} enqueue failed", notification.User.Id);
        }

        await Task.CompletedTask;
    }
    public class CreateUserEvent
    {
        public Ulid Id { get; set; }
        public string? Username { get; set; }

        public string? Password { get; set; }

        public Gender? Gender { get; set; }

        public UserStatus Status { get; set; }

        public string RoleId { get; set; }
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime? DayOfBirth { get; set; }

        public string? ProvinceId { get; set; }

        public string? DistrictId { get; set; }

        public string? CommuneId { get; set; }

        public string? Street { get; set; }

        public IFormFile? Avatar { get; set; }
    }
}
