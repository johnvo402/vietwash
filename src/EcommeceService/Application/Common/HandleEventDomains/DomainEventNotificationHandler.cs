using Application.Common.HandleEventDomains.Inventories;
using Application.Common.HandleEventDomains.Orders;
using Contracts.Application.Common.Events;
using Domain.Aggregates.Inventories.Events;
using Domain.Aggregates.Orders.Events;
using Domain.Events;
using Mediator;

namespace Application.Common.HandleEventDomains;

public sealed class DomainEventNotificationHandler(
    CreateFundEventHandler createFundHandler,
    InventoryDocumentCompletedHandler inventoryCompletedHandler,
    InventoryDocumentCanceledHandler inventoryCanceledHandler,
    EInvoiceEventHandler eInvoiceHandler
) : INotificationHandler<DomainEventNotification>
{
    public ValueTask Handle(
        DomainEventNotification notification,
        CancellationToken cancellationToken
    ) =>
        notification.DomainEvent switch
        {
            CreateFundEvent domainEvent => createFundHandler.Handle(domainEvent, cancellationToken),
            InventoryDocumentCompletedEvent domainEvent => inventoryCompletedHandler.Handle(
                domainEvent,
                cancellationToken
            ),
            InventoryDocumentCanceledEvent domainEvent => inventoryCanceledHandler.Handle(
                domainEvent,
                cancellationToken
            ),
            EInvoiceEvent domainEvent => eInvoiceHandler.Handle(domainEvent, cancellationToken),
            _ => ValueTask.CompletedTask,
        };
}
