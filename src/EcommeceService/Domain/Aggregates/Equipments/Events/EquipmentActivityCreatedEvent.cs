using Mediator;
namespace Domain.Aggregates.Equipments.Events;

public class EquipmentActivityCreatedEvent : INotification
{
	public EquipmentActivity EquipmentActivity { get; init; } = default!;
}
