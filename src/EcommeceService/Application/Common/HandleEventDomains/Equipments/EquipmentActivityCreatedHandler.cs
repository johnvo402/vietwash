using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Equipments.Events;
using Domain.Aggregates.Equipments;
using Mediator;

namespace Application.Common.HandleEventDomains.Equipments;

public sealed class EquipmentActivityCreatedHandler : INotificationHandler<EquipmentActivityCreatedEvent>
{
	private readonly IUnitOfWork _unitOfWork;

	public EquipmentActivityCreatedHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async ValueTask Handle(EquipmentActivityCreatedEvent notification, CancellationToken cancellationToken)
	{
		var activity = notification.EquipmentActivity;

		var equipment = await _unitOfWork
			.Repository<Equipment>()
			.FindByIdAsync(activity.EquipmentId, cancellationToken);

		if (equipment == null)
			return;

		equipment.Status = EquipmentStatus.Active;
		equipment.LastMaintenanceOrRepairDate = activity.PerformedDate;

		await _unitOfWork.Repository<Equipment>().UpdateAsync(equipment);
		await _unitOfWork.SaveAsync(cancellationToken);
	}
}
