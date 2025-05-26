using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.FundBehaviors;
using Domain.Aggregates.Funds;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;

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
               .WithState(x =>
                   Messager
                       .Create<FundBehavior>()
                       .Property(x => x.Name)
                       .Message(MessageType.Null)
                       .Negative()
                       .Build()
               )
               .MaximumLength(256)
               .WithState(x =>
                   Messager
                       .Create<FundBehavior>()
                       .Property(x => x.Name)
                       .Message(MessageType.MaximumLength)
                       .Build()
               );

            RuleFor(x => x.Type).IsInEnum().WithState(x =>
                    Messager
                        .Create<FundBehavior>()
                        .Property(x => x.Type)
                        .Message(MessageType.Valid)
                        .Build()
                );
        }


    }
}
