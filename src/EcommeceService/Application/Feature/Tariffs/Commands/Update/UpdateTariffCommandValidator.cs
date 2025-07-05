using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Tariffs;
using FluentValidation;


namespace Application.Feature.Tariffs.Commands.Update
{
    public class UpdateTariffCommandValidator : AbstractValidator<UpdateTariffCommand>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;

		public UpdateTariffCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
		{
			this.unitOfWork = unitOfWork;
			this.accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			RuleFor(x => x.Tariff)
				.SetValidator(new TariffValidator(unitOfWork, accessorService));
		}
	}
}