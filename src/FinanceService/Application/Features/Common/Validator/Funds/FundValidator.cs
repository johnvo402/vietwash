using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.Funds;
using Domain.Aggregates.Funds;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;

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
                .WithState(x =>
                    Messager
                        .Create<Fund>()
                        .Property(x => x.Name)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MaximumLength(256)
                .WithState(x =>
                    Messager
                        .Create<Fund>()
                        .Property(x => x.Name)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithState(x =>
                    Messager
                        .Create<Fund>()
                        .Property(x => x.Type)
                        .Message(MessageType.Valid)
                        .Build()
                );

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithState(x =>
                    Messager
                        .Create<Fund>()
                        .Property(x => x.Name)
                        .Message(MessageType.GreaterThan)
                        .Build()
                );

            RuleFor(x => x.FundBehaviorId)
                .MustAsync(FundBehaviorExists)
                .WithState(x =>
                    Messager
                        .Create<Fund>()
                        .Property(x => x.FundBehaviorId)
                        .Message(MessageType.Existence)
                        .Build()
                );

            RuleFor(x => x.Note)
                .MaximumLength(500)
                .WithState(x =>
                    Messager
                        .Create<Fund>()
                        .Property(x => x.Name)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(x => x.PaymentMethod)
                .IsInEnum()
                .WithState(x =>
                    Messager
                        .Create<Fund>()
                        .Property(x => x.PaymentMethod)
                        .Message(MessageType.Valid)
                        .Build()
                );
        }

        private async Task<bool> FundBehaviorExists(long id, CancellationToken ct)
        {
            return await _unitOfWork.Repository<FundBehavior>().AnyAsync(fb => fb.Id == id, ct);
        }
    }

}