using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validator.Funds;
using FluentValidation;

namespace Application.Features.FundBehaviors.Command
{
    public class CreateFundBehaviorCommandValidator : AbstractValidator<CreateFundBehaviorCommand>
    {
        public CreateFundBehaviorCommandValidator()
        {
            ApplyRules();
        }

        private void ApplyRules()
        {
            Include(new FundBehaviorValidator());
        }
    }
}
