using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Funds;

namespace Application.Features.Funds.Command.Update
{
    public class UpdateFundCommandValidator : AbstractValidator<UpdateFundCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _accessorService;

        public UpdateFundCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
        {
            _unitOfWork = unitOfWork;
            _accessorService = accessorService;

            ApplyRules();
        }


        private void ApplyRules()
        {
            RuleFor(x => x.updateFundModel!.Name!)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<UpdateFundCommand>()
                        .Property(x => x.updateFundModel!.Name!)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MaximumLength(256)
                .WithState(x =>
                    Messager
                        .Create<UpdateFundCommand>()
                        .Property(x => x.updateFundModel!.Name!)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );



            RuleFor(x => x.updateFundModel!.Amount!)
                .GreaterThan(0)
                .WithState(x =>
                    Messager
                        .Create<UpdateFundCommand>()
                        .Property(x => x.updateFundModel!.Amount!)
                        .Message(MessageType.GreaterThan)
                        .Build()
                );
            ;

            RuleFor(x => x.updateFundModel!.BehaviorId!)
                .MustAsync(FundBehaviorExists)
                  .WithState(x =>
                    Messager
                        .Create<Fund>()
                        .Property(x => x.FundBehaviorId)
                        .Message(MessageType.Existence)
                        .Build()
                );

            RuleFor(x => x.updateFundModel!.Note)
                .MaximumLength(500)
                .WithState(x =>
                    Messager
                        .Create<UpdateFundCommand>()
                        .Property(x => x.updateFundModel!.Note!)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );
            ;

            RuleFor(x => x.updateFundModel!.PaymentMethod).IsInEnum().WithState(x =>
                    Messager
                        .Create<UpdateFundCommand>()
                        .Property(x => x.updateFundModel!.PaymentMethod)
                        .Message(MessageType.Valid)
                        .Build()
                ); ;
        }


        private async Task<bool> FundBehaviorExists(long id, CancellationToken ct)
        {
            return await _unitOfWork.Repository<FundBehavior>().AnyAsync(fb => fb.Id == id, ct);
        }
    }
}
