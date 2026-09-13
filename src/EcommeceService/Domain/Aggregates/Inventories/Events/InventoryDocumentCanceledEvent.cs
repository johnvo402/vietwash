using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Inventories.Events;

public class InventoryDocumentCanceledEvent : IDomainEvent
{
    public InventoryDocument InventoryDocument { get; init; } = default!;
}
