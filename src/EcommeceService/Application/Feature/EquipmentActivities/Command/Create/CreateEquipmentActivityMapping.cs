using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Equipments;
using Contracts.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Application.Feature.EquipmentActivities.Command.Create;

public static class CreateEquipmentActivityMapping
{
	public static EquipmentActivity ToEntity(this CreateEquipmentActivityCommand cmd, long staffId, string supervisorCode)
	{
		var initialStatus = cmd.Type == TypeActivity.Repair
		? ActivityStatus.InProgress
		: ActivityStatus.Scheduled;
		decimal amount = cmd.Details.Sum(d => d.UnitPrice * d.Quantity);

		var response = new EquipmentActivity(
			equipmentId: cmd.EquipmentId,
			branchId: cmd.BranchId,
			staffId: staffId,
			type: cmd.Type,
			reportedDate: cmd.ReportedDate,
			scheduledDate: cmd.Type == TypeActivity.Maintenance ? cmd.ScheduledDate : null,
			laborCost: cmd.LaborCost,
			totalCost: amount + cmd.LaborCost,
			description: cmd.Description,
			supervisorCode: supervisorCode,
			status: initialStatus
		);
		response.ActivityDetails = cmd.Details.ToListMapping(x => new EquipmentActivityDetail
		{
			PartName = x.PartName,
			Quantity = x.Quantity,
			UnitPrice = x.UnitPrice,
			Amount = x.Quantity * x.UnitPrice
		});

		return response;
	}
}
