using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.EquipmentActivities;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using FluentValidation;

namespace Application.Feature.EquipmentActivities.Command.Create
{
	public class CreateEquipmentActivityCommandValidator : AbstractValidator<CreateEquipmentActivityCommand>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IActionAccessorService _accessorService;

		public CreateEquipmentActivityCommandValidator(
			IUnitOfWork unitOfWork,
			IActionAccessorService accessorService
		)
		{
			_unitOfWork = unitOfWork;
			_accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			Include(new EquipmentActivityValidator(_unitOfWork, _accessorService));
			RuleFor(x => x.EquipmentId)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<CreateEquipmentActivityCommand>()
						.Property(x => x.EquipmentId)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				)
				.MustAsync(IsEquipmentExistsAsync)
				.WithState(_ =>
					Messager
						.Create<EquipmentActivity>()
						.Property(x => x.Id)
						.Message(MessageType.Found)
						.Negative()
						.Build()
				);
			RuleFor(x => x.Type)
				.MustAsync(MatchActivityTypeWithEquipmentStatusAsync)
				.WithState(_ =>
					Messager
						.Create<EquipmentActivity>()
						.Property(x => x.Type)
						.Message(MessageType.Matching)
						.Negative()
						.Build()
				);

		}
		private async Task<bool> IsEquipmentExistsAsync(CreateEquipmentActivityCommand command, long equipmentId, CancellationToken cancellation)
		{
			return await _unitOfWork
				.Repository<Equipment>()
				.AnyAsync(x => x.Id == equipmentId && x.BranchId == command.BranchId, cancellation);
		}

		private async Task<bool> MatchActivityTypeWithEquipmentStatusAsync(CreateEquipmentActivityCommand command, TypeActivity type, CancellationToken cancellation)
		{
			var equipment = await _unitOfWork
				.Repository<Equipment>()
				.FindByConditionAsync(x => x.Id == command.EquipmentId && x.BranchId == command.BranchId, cancellation);

			return equipment.Status switch
			{
				EquipmentStatus.UnderMaintenance => type == TypeActivity.Maintenance,
				EquipmentStatus.UnderRepair => type == TypeActivity.Repair,
				_ => true // không giới hạn type cho các status khác
			};
		}
	}
}
