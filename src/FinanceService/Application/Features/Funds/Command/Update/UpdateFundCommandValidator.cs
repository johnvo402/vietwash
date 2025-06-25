using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Funds;
using FluentValidation;

namespace Application.Features.Funds.Command.Update
{
    public class UpdateFundCommandValidator : AbstractValidator<UpdateFundCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _accessorService;

        public UpdateFundCommandValidator(
            IUnitOfWork unitOfWork,
            IActionAccessorService accessorService
        )
        {
            _unitOfWork = unitOfWork;
            _accessorService = accessorService;

            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.UpdateFundModel!.PaymentMethod)
                .IsInEnum()
                .WithState(x =>
                    Messager
                        .Create<UpdateFundCommand>()
                        .Property(x => x.UpdateFundModel!.PaymentMethod)
                        .Message(MessageType.Valid)
                        .Build()
                );
            ;
        }

        private async Task<bool> FundBehaviorExists(long id, CancellationToken ct)
        {
            return await _unitOfWork.Repository<FundBehavior>().AnyAsync(fb => fb.Id == id, ct);
        }
    }
}
