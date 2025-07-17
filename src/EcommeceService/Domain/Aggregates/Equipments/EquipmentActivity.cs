using Ardalis.GuardClauses;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Users;
using Shared.Kernel.Common;
using Domain.Aggregates.Equipments.Events;
using Mediator;
using Domain.Events;

namespace Domain.Aggregates.Equipments
{
	public class EquipmentActivity : AggregateRoot
	{
		public long EquipmentId { get; set; } = default!;
		public long BranchId { get; set; } = default!;
		public long StaffId { get; set; } = default!;
		public TypeActivity Type { get; set; } = default!;
		public DateTimeOffset? PerformedDate { get; set; } = default!;
		public decimal LaborCost { get; set; } // tiền công 
		public decimal TotalCost { get; set; } = default!;
		public string? Description { get; set; } = default!;
		public string SupervisorCode { get; set; } = default!;
		public Equipment? Equipment { get; set; }
		public User? Staff { get; set; }
		public ICollection<EquipmentActivityDetail> ActivityDetails { get; set; } = [];

		public EquipmentActivity(
			long equipmentId,
			long branchId,
			long staffId,
			TypeActivity type,
			DateTimeOffset? performedDate,
			decimal laborCost,
			decimal totalCost,
			string? description,
			string supervisorCode
		)
		{
			Guard.Against.NegativeOrZero(equipmentId, nameof(equipmentId));
			Guard.Against.NegativeOrZero(branchId, nameof(branchId));
			Guard.Against.NegativeOrZero(staffId, nameof(staffId));

			EquipmentId = equipmentId;
			BranchId = branchId;
			StaffId = staffId;
			Type = type;
			PerformedDate = performedDate;
			LaborCost = laborCost;
			TotalCost = totalCost;
			Description = description;
			SupervisorCode = supervisorCode;

			Emit(new EquipmentActivityCreatedEvent { EquipmentActivity = this });
		}

		protected override bool TryApplyDomainEvent(INotification domainEvent)
		{
			switch (domainEvent)
			{
				case EquipmentActivityCreatedEvent:
					return true;
				default:
					return false;
			}
		}
	}
}
