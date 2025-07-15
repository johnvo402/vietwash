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
		public DateTimeOffset? ReportedDate { get; set; } = default!;
		public DateTimeOffset? ScheduledDate { get; set; } = default!;
		public decimal LaborCost { get; set; } // tiền công
		public decimal TotalCost { get; set; } = default!;
		public string? Description { get; set; } = default!;
		public string? ReceivedBy { get; set; } = default!;
		public string SupervisorCode { get; set; } = default!;
		public ActivityStatus Status { get; set; } = default!;
		public Equipment? Equipment { get; set; }
		public User? Staff { get; set; }
		public ICollection<EquipmentActivityDetail> ActivityDetails { get; set; } = [];

		public EquipmentActivity(
			long equipmentId,
			long branchId,
			long staffId,
			TypeActivity type,
			DateTimeOffset? reportedDate,
			DateTimeOffset? scheduledDate,
			decimal laborCost,
			decimal totalCost,
			string? description,
			string supervisorCode,
			ActivityStatus status
		)
		{
			Guard.Against.NegativeOrZero(equipmentId, nameof(equipmentId));
			Guard.Against.NegativeOrZero(branchId, nameof(branchId));
			Guard.Against.NegativeOrZero(staffId, nameof(staffId));

			EquipmentId = equipmentId;
			BranchId = branchId;
			StaffId = staffId;
			Type = type;
			ReportedDate = reportedDate;
			ScheduledDate = scheduledDate;
			LaborCost = laborCost;
			TotalCost = totalCost;
			Description = description;
			SupervisorCode = supervisorCode;
			Status = status;

			Emit(new EquipmentActivityCreatedEvent { EquipmentActivity = this });
		}

		public void Update(
			long? branchId = null,
			long? staffId = null,
			DateTimeOffset? reportedDate = null,
			DateTimeOffset? scheduledDate = null,
			decimal? laborCost = null,
			decimal? totalCost = null,
			string? description = null,
			string? supervisorCode = null
		)
		{
			if (branchId.HasValue)
				BranchId = branchId.Value;
			if (staffId.HasValue)
				StaffId = staffId.Value;
			if (reportedDate.HasValue)
				ReportedDate = reportedDate.Value;
			if (scheduledDate.HasValue)
				ScheduledDate = scheduledDate.Value;
			if (laborCost.HasValue)
				LaborCost = laborCost.Value;
			if (totalCost.HasValue)
				TotalCost = totalCost.Value;
			if (supervisorCode is not null)
				SupervisorCode = supervisorCode;
			if (description is not null)
				Description = description;

			Emit(new EquipmentActivityCreatedEvent { EquipmentActivity = this });

		}
		public void UpdateStatus(ActivityStatus newStatus)
		{
			Status = newStatus;
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
