using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using FluentValidation;
using Domain.Aggregates.Funds.Enums;
using System;
using System.Threading.Tasks;
using Application.Features.Common.Projections.Funds;
using Domain.Aggregates.Funds;

namespace Application.Features.Common.Validator.Funds
{
    public class FundValidator : AbstractValidator<CreateFundModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _accessorService;

        public FundValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
        {
            _unitOfWork = unitOfWork;
            _accessorService = accessorService;

            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Fund name must not be empty.")
                .MaximumLength(200)
                .WithMessage("Fund name must not exceed 200 characters.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid fund type.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.FundBehaviorId)
                             .MustAsync(async (id, ct) =>
                    await _unitOfWork.Repository<FundBehavior>().AnyAsync(fb => fb.Id == id, ct))
                .WithMessage("Fund behavior does not exist.");



            RuleFor(x => x.Note)
                .MaximumLength(500)
                .WithMessage("Note must not exceed 500 characters.");

            RuleFor(x => x.PaymentMethod)
                .IsInEnum()
                .WithMessage("Invalid payment method.");


        }
    }
}
