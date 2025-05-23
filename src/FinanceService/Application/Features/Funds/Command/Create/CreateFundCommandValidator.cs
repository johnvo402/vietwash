
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validator.Funds;
using FluentValidation;

namespace Application.Features.Funds.Command.Create
{
    public class CreateFundCommandValidator : AbstractValidator<CreateFundCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _accessorService;

        public CreateFundCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
        {
            _unitOfWork = unitOfWork;
            _accessorService = accessorService;

            ApplyRules();
        }

        private void ApplyRules()
        {

            Include(new FundValidator(_unitOfWork, _accessorService));


        }
    }
}
