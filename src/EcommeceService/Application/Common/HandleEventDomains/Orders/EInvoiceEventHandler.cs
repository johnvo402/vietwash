using Application.Common.Interfaces.Services.DistributedCache;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.PubSubLogs;
using Mediator;
using Serilog;

namespace Application.Common.HandleEventDomains.Orders
{
    public class EInvoiceEventHandler(ILogger logger, IPubSubFactory queueFactory)
        : INotificationHandler<EInvoiceEvent>
    {
        public async ValueTask Handle(
            EInvoiceEvent notification,
            CancellationToken cancellationToken
        )
        {
            if (notification.Order is null)
            {
                logger.Warning("EInvoiceEventHandler received an event without an order");
                return;
            }

            logger.Information("EInvoiceEventHandler: {@Id}", notification.Order.Id);
            var data = notification.Order.ToEInvoiceMessage();
            var check = await queueFactory
                .GetPubSub(PubSubType.Origin)
                .PublishAsync(data, "EInvoiceEvent");
            if (!check)
            {
                logger.Error(
                    "EInvoiceEventHandler: {@Id} enqueue failed",
                    notification.Order.Id
                );
            }
        }
    }
}
