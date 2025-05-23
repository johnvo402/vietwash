using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validator.Funds;
using Application.Features.Funds.Command.Create;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FundBehaviors.Command
{
    public class CreateFundBehaviorCommandValidator : AbstractValidator<CreateFundBehaviorCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _accessorService;

        public CreateFundBehaviorCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
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
