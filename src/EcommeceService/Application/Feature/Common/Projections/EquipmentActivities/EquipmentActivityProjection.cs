using Contracts.Application.Common;
using Domain.Aggregates.Equipments.Enums;

namespace Application.Feature.Common.Projections.EquipmentActivities
{
	public class EquipmentActivityProjection : BaseResponse
	{
		public long EquipmentId { get; set; }
		public string? EquipmentName { get; set; }
		public long BranchId { get; set; }
		public long StaffId { get; set; }
		public string SupervisorName { get; set; } = default!;
		public string SupervisorCode { get; set; } = default!;
		public TypeActivity Type { get; set; }
		public decimal LaborCost { get; set; }
		public decimal TotalCost { get; set; }
		public string? Description { get; set; }
	}
}
