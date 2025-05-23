using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using FluentValidation;
using System.Threading.Tasks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;

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
            RuleFor(x => x.FundId)
                .NotEmpty()
                .WithState(x => Messager.Create<UpdateFundCommand>(nameof(Fund)).Property(x => x.FundId).Message(MessageType.Null).Negative().Build())
                .Must(id => long.TryParse(id, out _))
                .WithState(x => Messager.Create<UpdateFundCommand>(nameof(Fund)).Property(x => x.FundId).Message(MessageType.Valid).Negative().Build())
                .MustAsync(async (id, ct) =>
                    await _unitOfWork.Repository<Fund>().AnyAsync(f => f.Id == long.Parse(id), ct))
                .WithState(x => Messager.Create<UpdateFundCommand>(nameof(Fund)).Property(x => x.FundId).Message(MessageType.Existence).Negative().Build());

            RuleFor(x => x.updateFundModel)
                .NotNull()
                .WithState(x => Messager.Create<UpdateFundCommand>(nameof(Fund)).Property(x => x.updateFundModel).Message(MessageType.Null).Negative().Build());

        }
    }
}
