using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Equipments;
using FluentValidation;

namespace Application.Feature.Equipments.Command.Create
{
	public class CreateEquipmentCommandValidator : AbstractValidator<CreateEquipmentCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		private readonly IActionAccessorService _accessorService;

		public CreateEquipmentCommandValidator(
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
			Include(new EquipmentValidator(_unitOfWork, _accessorService));
		}
	}
}
