using Domain.Aggregates.Equipments;
using Contracts.Extensions;

namespace Application.Feature.EquipmentActivities.Command.Create;

public static class CreateEquipmentActivityMapping
{
	public static EquipmentActivity ToEntity(this CreateEquipmentActivityCommand cmd, long staffId, string supervisorCode)
	{
	
		decimal amount = cmd.Details.Sum(d => d.UnitPrice * d.Quantity);

		var response = new EquipmentActivity(
			equipmentId: cmd.EquipmentId,
			branchId: cmd.BranchId,
			staffId: staffId,
			type: cmd.Type,
			performedDate: DateTimeOffset.UtcNow,
			laborCost: cmd.LaborCost,
			totalCost: amount + cmd.LaborCost,
			description: cmd.Description,
			supervisorCode: supervisorCode
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
