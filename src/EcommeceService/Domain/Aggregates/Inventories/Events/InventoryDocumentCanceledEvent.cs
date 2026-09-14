using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Inventories.Events;

public sealed record InventoryDocumentCanceledEvent(
    string DocumentCode,
    IReadOnlyList<string> EquipmentCodes
) : IDomainEvent;
