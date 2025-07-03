using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Equipments;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using FluentValidation;

namespace Application.Feature.Equipments.Command.Update
{
	public class UpdateEquipmentCommandValidator : AbstractValidator<UpdateEquipmentCommand>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;

		public UpdateEquipmentCommandValidator(
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
			RuleFor(x => x.Equipment)
				.SetValidator(new EquipmentValidator(unitOfWork, accessorService));
			RuleFor(x => x.EquipmentId)
				.NotEmpty()
				.WithState(x =>
						Messager
							.Create<Equipment>()
							.Property(x => x.Id)
							.Message(MessageType.Null)
							.Negative()
							.Build()
				)
				.MustAsync(IsEquipmentExistsAsync)
				.WithState(x =>
						Messager
							.Create<Equipment>()
							.Property(x => x.Id)
							.Message(MessageType.Found)
							.Negative()
							.Build()
				);
		}
		private async Task<bool> IsEquipmentExistsAsync(long equipmentId, CancellationToken cancellation)
		{
			return await unitOfWork.Repository<Equipment>().AnyAsync(s => s.Id == equipmentId, cancellation);
		}
	}
}
