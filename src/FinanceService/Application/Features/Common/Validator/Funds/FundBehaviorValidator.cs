using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.FundBehaviors;
using Contracts.Common.Messages;
using Domain.Aggregates.Funds;
using FluentValidation;

namespace Application.Features.Common.Validator.Funds
{
    public class FundBehaviorValidator : AbstractValidator<CreateFundBehaviorModel>
    {
        public FundBehaviorValidator()
        {
            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.Name)
                .NotNull()
                .WithState(x =>
                    Messager
                        .Create<FundBehavior>()
                        .Property(x => x.Name)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithState(x =>
                    Messager
                        .Create<FundBehavior>()
                        .Property(x => x.Type)
                        .Message(MessageType.Valid)
                        .Build()
                );
        }
    }
}
