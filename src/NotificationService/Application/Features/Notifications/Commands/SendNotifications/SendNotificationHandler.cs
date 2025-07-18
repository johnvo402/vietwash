using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Notifications;
using Mediator;

namespace Application.Features.Notifications.Commands.SendNotifications
{
    public class SendNotificationHandler(INotificationService notification)
        : IRequestHandler<SendNotificationCommand, Result>
    {
        public async ValueTask<Result> Handle(
            SendNotificationCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                await notification.SendAsync(request, cancellationToken);
                return Result.Success();
            }
            catch
            {
                return Result.Failure(
                    new BadRequestError(
                        "Error has occured with notification",
                        Messager
                            .Create<SendNotificationCommand>(nameof(Notification))
                            .Message(MessageType.Valid)
                            .Negative()
                            .Build()
                    )
                );
            }
        }
    }
}
