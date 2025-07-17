using Mediator;

namespace Domain.Aggregates.Inventories.Events;

public class InventoryDocumentCompletedEvent : INotification
{
    public InventoryDocument InventoryDocument { get; init; } = default!;
}
