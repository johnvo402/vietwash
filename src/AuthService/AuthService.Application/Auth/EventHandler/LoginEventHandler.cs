using AuthService.Application.Interfaces;
using AuthService.Domain.UserActivities;
using AuthService.Domain.Users.Events;
using AuthService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.EventHandler
{
    public class LoginEventHandler : INotificationHandler<UserLoggedInEvent>
    {
        private readonly IUserActivityRepo _activityRepo;
        public LoginEventHandler(IUserActivityRepo activityRepo)
        {
            _activityRepo = activityRepo;
        }


        public async Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
        {
            if (notification.id == null)
            {
                return;
            }

            var userActivity = new UserActivity
            {
                ActivityType = ActivityTypes.Login,
                UserId = Guid.Parse(notification.id),
                CreatedAt = notification.OccurredOn,
                ActivityDate = notification.OccurredOn
            };
            await _activityRepo.CreateAsync(userActivity, cancellationToken);
        }
    }
}
