
using Application.Feature.Common.Projections.EquipmentActivities;
using Application.Feature.Common.Projections.Tariffs;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Tariffs;

namespace Application.Feature.Common.Mapping.EquipmentActivities;

public static class EquipmentActivityMapping
{
	public static List<EquipmentActivityDetail>? ToListActivityDetails(
		this List<EquipmentActivityDetailModel>? activityDetails
	) => activityDetails?.Select(ToActivityDetailsEntity).ToList();

	public static EquipmentActivityDetail ToActivityDetailsEntity(this EquipmentActivityDetailModel model)
	{
		return new EquipmentActivityDetail
		{
			PartName = model.PartName,
			Quantity = model.Quantity,
			UnitPrice = model.UnitPrice,
			Amount = model.Quantity * model.UnitPrice
		};
	}
}
