using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Notifications;
using Mediator;

namespace Application.Features.Notifications.Commands.ReadNotify
{
    public class ReadNotifyHandler(INotificationService notification)
        : IRequestHandler<ReadNotifyCommand, Result>
    {
        public async ValueTask<Result> Handle(
            ReadNotifyCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                await notification.ReadAsync(request.Id);
                return Result.Success();
            }
            catch
            {
                return Result.Failure(
                    new BadRequestError(
                        "Error has occured with notification",
                        Messager
                            .Create<ReadNotifyCommand>(nameof(Notification))
                            .Message(MessageType.Valid)
                            .Negative()
                            .Build()
                    )
                );
            }
        }
    }
}
