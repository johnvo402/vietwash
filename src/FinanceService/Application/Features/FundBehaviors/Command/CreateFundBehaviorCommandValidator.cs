using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validator.Funds;
using FluentValidation;

namespace Application.Features.FundBehaviors.Command
{
    public class CreateFundBehaviorCommandValidator : AbstractValidator<CreateFundBehaviorCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _accessorService;

        public CreateFundBehaviorCommandValidator(
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
            Include(new FundBehaviorValidator(_unitOfWork, _accessorService));
        }
    }
}
