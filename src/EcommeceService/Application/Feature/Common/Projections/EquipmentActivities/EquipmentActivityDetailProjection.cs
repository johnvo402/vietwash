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
			Image = activity.Equipment?.Image;
			BranchId = activity.BranchId;
			StaffId = activity.StaffId;
			Type = activity.Type;
			ReportedDate = activity.ReportedDate;
			ScheduledDate = activity.ScheduledDate;
			LaborCost = activity.LaborCost;
			TotalCost = activity.TotalCost;
			Description = activity.Description;
			SupervisorCode = activity.SupervisorCode;
			Status = activity.Status;

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
