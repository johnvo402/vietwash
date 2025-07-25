using Application.Features.Common.Projections;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Notifications.Commands.SendNotifications
{
    public class SendNotificationCommand : NotificationModel, IRequest<Result>;
}
