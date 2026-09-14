using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Inventories.Events;

public sealed record InventoryDocumentCompletedEvent(long InventoryDocumentId) : IDomainEvent;
