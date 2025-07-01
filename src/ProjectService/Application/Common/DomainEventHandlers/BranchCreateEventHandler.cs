using Application.Common.Interfaces.Services.DistributedCache;
using Domain.Aggregates.Branches.Events;
using Domain.Aggregates.PubSubLogs;
using Mediator;
using Serilog;

namespace Application.Common.DomainEventHandlers
{
    public class BranchCreateEventHandler(ILogger logger, IPubSubFactory queueFactory)
        : INotificationHandler<BranchCreateEvent>
    {
        public async ValueTask Handle(
            BranchCreateEvent notification,
            CancellationToken cancellationToken
        )
        {
            logger.Information("BranchCreateEventHandler: {@Id}", notification.BranchId);

            var check = await queueFactory
                .GetPubSub(PubSubType.Origin)
                .PublishAsync(notification, "branch-create-event");
            if (!check)
            {
                logger.Error(
                    "BranchCreateEventHandler: {@BranchId} enqueue failed",
                    notification.BranchId
                );
            }

            await Task.CompletedTask;
        }
    }
}
