using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.EquipmentActivities;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
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
						.Create<Equipment>()
						.Property(x => x.Id)
						.Message(MessageType.Found)
						.Negative()
						.Build()
				);
		}
		private async Task<bool> IsEquipmentExistsAsync(CreateEquipmentActivityCommand command, long quipmentId, CancellationToken cancellation)
		{
			return await _unitOfWork
				.Repository<Equipment>()
				.AnyAsync(x => x.Id == quipmentId && x.BranchId == command.BranchId, cancellation);
		}
	}
}
