using Domain.Aggregates.Equipments.Events;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Products.Events;
using Mediator;

namespace Application.Common.HandleEventDomains;

/// <summary>
/// Acknowledges domain events whose side effects are captured by infrastructure
/// (for example, the transactional outbox) or which currently act as lifecycle markers.
/// </summary>
public sealed class AcknowledgeMarkerEventsHandler
    : INotificationHandler<EquipmentActivityCreatedEvent>,
        INotificationHandler<UpdateStatusOrderEvent>,
        INotificationHandler<BranchProductCreateEvent>
{
    public ValueTask Handle(
        EquipmentActivityCreatedEvent notification,
        CancellationToken cancellationToken
    ) => ValueTask.CompletedTask;

    public ValueTask Handle(
        UpdateStatusOrderEvent notification,
        CancellationToken cancellationToken
    ) => ValueTask.CompletedTask;

    public ValueTask Handle(
        BranchProductCreateEvent notification,
        CancellationToken cancellationToken
    ) => ValueTask.CompletedTask;
}
