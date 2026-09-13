using Domain.Aggregates.Orders.Events;
using Mediator;

namespace Application.Common.HandleEventDomains.Orders
{
    public class EInvoiceEventHandler
    {
        public ValueTask Handle(
            EInvoiceEvent notification,
            CancellationToken cancellationToken
        )
        {
            throw new InvalidOperationException(
                "Order EInvoice events must be persisted and dispatched through IntegrationOutbox."
            );
        }
    }
}
