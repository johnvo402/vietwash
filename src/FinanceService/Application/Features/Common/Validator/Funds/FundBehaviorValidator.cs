using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.FundBehaviors;
using Application.Features.Common.Projections.Funds;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Common.Validator.Funds
{
    public class FundBehaviorValidator : AbstractValidator<CreateFundBehaviorModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _accessorService;

        public FundBehaviorValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
        {
            _unitOfWork = unitOfWork;
            _accessorService = accessorService;

            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Fund behavior name must not be empty.")
                .MaximumLength(200)
                .WithMessage("Fund behavior name must not exceed 200 characters.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid fund type.");


        }
    }
}
