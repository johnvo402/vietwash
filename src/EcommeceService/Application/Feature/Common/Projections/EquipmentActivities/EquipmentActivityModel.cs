using Domain.Aggregates.Equipments.Enums;

namespace Application.Feature.Common.Projections.EquipmentActivities
{
	public class EquipmentActivityModel
	{
		public long BranchId { get; set; }
		public DateTimeOffset? ReportedDate { get; set; }
		public DateTimeOffset? ScheduledDate { get; set; } 
		public string? Description { get; set; }
		public decimal LaborCost { get; set; }
		public List<EquipmentActivityDetailModel> Details { get; set; } = [];
	}
}
