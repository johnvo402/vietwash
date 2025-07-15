using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.EquipmentActivities;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using FluentValidation;

namespace Application.Feature.EquipmentActivities.Command.Update
{
	public class UpdateEquipmentActivityCommandValidator : AbstractValidator<UpdateEquipmentActivityCommand>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;

		public UpdateEquipmentActivityCommandValidator(
			IUnitOfWork unitOfWork,
			IActionAccessorService accessorService
		)
		{
			this.unitOfWork = unitOfWork;
			this.accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			RuleFor(x => x.EquipmentActivity)
				.SetValidator(new EquipmentActivityValidator(unitOfWork, accessorService));
			RuleFor(x => x.EquipmentActivityId)
				.NotEmpty()
				.WithState(x =>
						Messager
							.Create<EquipmentActivity>()
							.Property(x => x.Id)
							.Message(MessageType.Null)
							.Negative()
							.Build()
				)
				.MustAsync(IsEquipmentActivityExistsAsync)
				.WithState(x =>
						Messager
							.Create<EquipmentActivity>()
							.Property(x => x.Id)
							.Message(MessageType.Found)
							.Negative()
							.Build()
				);
		}
		private async Task<bool> IsEquipmentActivityExistsAsync(UpdateEquipmentActivityCommand cmd, long equipmentActivityId, CancellationToken cancellation)
		{
			return await unitOfWork.Repository<EquipmentActivity>().AnyAsync(x => x.BranchId == cmd.EquipmentActivity.BranchId && x.Id == equipmentActivityId, cancellation);
		}
	}
}
