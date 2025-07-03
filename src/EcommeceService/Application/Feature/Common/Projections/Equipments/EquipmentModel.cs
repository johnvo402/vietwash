using Domain.Aggregates.Equipments.Enums;

namespace Application.Feature.Common.Projections.Equipments
{
	public class EquipmentModel
	{
		public long BranchId { get; set; }
		public string Name { get; set; } = default!;
		public string Description { get; set; } = default!;
		public string? Image { get; set; }
		public string Code { get; set; } = default!;
		public EquipmentType Type { get; set; } = default!; 
		public decimal Price { get; set; } = default!;
		public decimal Capacity { get; set; } = default!;
		public EquipmentStatus Status { get; set; } = EquipmentStatus.Active; 
		public DateTimeOffset? LastMaintenanceDate { get; set; }
		public DateTimeOffset? NextMaintenanceDate { get; set; }
	}
}
