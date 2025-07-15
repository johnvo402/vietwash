using Mediator;

namespace Domain.Aggregates.Inventories.Events;

public class InventoryDocumentCanceledEvent : INotification
{
    public InventoryDocument InventoryDocument { get; init; } = default!;
}
