using Domain.Aggregates.Equipments;

namespace Application.Feature.Common.Projections.EquipmentActivities
{
	public class EquipmentActivityDetailProjection : EquipmentActivityProjection
	{
		public ICollection<EquipmentActivityDetailItem> Details { get; set; } = [];
		public virtual void MappingFrom(EquipmentActivity activity)
		{
			Id = activity.Id;
			PublicId = activity.PublicId;
			CreatedAt = activity.CreatedAt;
			CreatedBy = activity.CreatedBy;
			UpdatedAt = activity.UpdatedAt;
			UpdatedBy = activity.UpdatedBy;

			EquipmentId = activity.EquipmentId;
			EquipmentName = activity.Equipment?.Name;
			BranchId = activity.BranchId;
			StaffId = activity.StaffId;
			Type = activity.Type;
			LaborCost = activity.LaborCost;
			TotalCost = activity.TotalCost;
			Description = activity.Description;
			SupervisorName = activity.Staff.DisplayName;
			SupervisorCode = activity.Staff.Code;

			Details = activity.ActivityDetails.Select(d => new EquipmentActivityDetailItem
			{
				PartName = d.PartName,
				Quantity = d.Quantity,
				UnitPrice = d.UnitPrice,
				Amount = d.Amount
			}).ToList();
		}

	}

	public class EquipmentActivityDetailItem
	{
		public string PartName { get; set; } = default!;
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal Amount { get; set; }
	}
}
