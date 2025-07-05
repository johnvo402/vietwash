using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Tariffs;
using FluentValidation;

namespace Application.Feature.Tariffs.Commands.Create
{
    public partial class CreateTariffCommandValidator : AbstractValidator<CreateTariffCommand>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;

        public CreateTariffCommandValidator(
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
            Include(new TariffValidator(unitOfWork, accessorService));
        }
    }
}
