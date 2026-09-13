using Application.Common.Interfaces.Services.DistributedCache;
using Domain.Aggregates.PubSubLogs;
using Domain.Events;
using Domain.Events.Enums;
using Mediator;
using Serilog;

namespace Application.Common.HandleEventDomains
{
    public class CreateFundEventHandler(ILogger logger, IPubSubFactory queueFactory)
        : INotificationHandler<CreateFundEvent>
    {
        public async ValueTask Handle(
            CreateFundEvent notification,
            CancellationToken cancellationToken
        )
        {
            if (notification.FundEventType == FundEventType.Order)
                throw new InvalidOperationException(
                    "Order finance events must be persisted and dispatched through IntegrationOutbox."
                );

            logger.Information("CreateFundEventHandler: {@Id}", notification.ReferenceId);

            var check = await queueFactory
                .GetPubSub(PubSubType.Origin)
                .PublishAsync(notification, "CreateFundEvent");
            if (!check)
            {
                logger.Error(
                    "CreateFundEventHandler: {@User} enqueue failed",
                    notification.ReferenceId
                );
            }

            await Task.CompletedTask;
        }
    }
}
