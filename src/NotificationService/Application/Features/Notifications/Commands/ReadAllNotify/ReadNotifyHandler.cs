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

namespace Application.Features.Notifications.Commands.ReadAllNotify
{
    public class ReadAllNotifyHandler(INotificationService notification, ICurrentAccount current)
        : IRequestHandler<ReadAllNotifyCommand, Result>
    {
        public async ValueTask<Result> Handle(
            ReadAllNotifyCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var id = current.Id.ToString();
                if (string.IsNullOrEmpty(id))
                {
                    return Result.Failure(
                        new BadRequestError(
                            "Error has occured with notification",
                            Messager
                                .Create<ReadAllNotifyCommand>(nameof(Notification))
                                .Message(MessageType.Valid)
                                .Negative()
                                .Build()
                        )
                    );
                }
                await notification.ReadAllAsync(id);
                return Result.Success();
            }
            catch
            {
                return Result.Failure(
                    new BadRequestError(
                        "Error has occured with notification",
                        Messager
                            .Create<ReadAllNotifyCommand>(nameof(Notification))
                            .Message(MessageType.Valid)
                            .Negative()
                            .Build()
                    )
                );
            }
        }
    }
}
