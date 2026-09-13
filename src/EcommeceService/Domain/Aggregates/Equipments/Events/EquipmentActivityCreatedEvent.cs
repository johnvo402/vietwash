using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Equipments.Events;

public class EquipmentActivityCreatedEvent : IDomainEvent
{
    public EquipmentActivity EquipmentActivity { get; init; } = default!;
}
