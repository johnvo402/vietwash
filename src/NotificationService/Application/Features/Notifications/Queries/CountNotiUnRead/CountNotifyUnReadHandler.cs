using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Notifications;
using Mediator;

namespace Application.Features.Notifications.Queries.CountNotiUnRead
{
    public class CountNotifyUnReadHandler(
        INotificationService notification,
        ICurrentAccount current
    ) : IRequestHandler<CountNotifyUnReadQuery, Result<CountNotifyUnReadResponse>>
    {
        public async ValueTask<Result<CountNotifyUnReadResponse>> Handle(
            CountNotifyUnReadQuery request,
            CancellationToken cancellationToken
        )
        {
            var id = current.Id.ToString();
            if (string.IsNullOrEmpty(id))
            {
                return Result<CountNotifyUnReadResponse>.Failure(
                    new BadRequestError(
                        "Error has occured with notification",
                        Messager
                            .Create<CountNotifyUnReadQuery>(nameof(Notification))
                            .Message(MessageType.Valid)
                            .Negative()
                            .Build()
                    )
                );
            }
            var numbers = await notification.GetUnreadCountAsync(id);
            return Result<CountNotifyUnReadResponse>.Success(new() { NumberNotify = numbers });
        }
    }
}
