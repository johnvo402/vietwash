using Application.Common.Interfaces.Services.DistributedCache;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.PubSubLogs;
using Mediator;
using Serilog;

namespace Application.Common.HandleEventDomains
{
    public class UpdateStatusOrderEventHandler(ILogger logger, IPubSubFactory queueFactory)
        : INotificationHandler<UpdateStatusOrderEvent>
    {
        public async ValueTask Handle(
            UpdateStatusOrderEvent notification,
            CancellationToken cancellationToken
        )
        {
            logger.Information("UpdateStatusOrderEventHandler: {@Id}", notification.OrderId);

            var check = await queueFactory
                .GetPubSub(PubSubType.Origin)
                .PublishAsync(notification, "UpdateStatusOrderEvent");
            if (!check)
            {
                logger.Error(
                    "UpdateStatusOrderEventHandler: {@User} enqueue failed",
                    notification.OrderId
                );
            }

            await Task.CompletedTask;
        }
    }
}
