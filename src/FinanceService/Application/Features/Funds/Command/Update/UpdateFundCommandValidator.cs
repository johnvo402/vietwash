using Application.Common.Interfaces.Services;
using Contracts.Common.Messages;
using Domain.Aggregates.Funds;
using FluentValidation;

namespace Application.Features.Funds.Command.Update
{
    public class UpdateFundCommandValidator : AbstractValidator<UpdateFundCommand>
    {
        private ICurrentAccount _currentAccount;

        public UpdateFundCommandValidator(ICurrentAccount currentAccount)
        {
            _currentAccount = currentAccount;

            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.UpdateFundModel!.PaymentMethod)
                .IsInEnum()
                .WithState(x =>
                    Messager
                        .Create<UpdateFundCommand>(nameof(Fund))
                        .Property(x => x.UpdateFundModel!.PaymentMethod)
                        .Message(MessageType.Valid)
                        .Build()
                );

            RuleFor(x => x.UpdateFundModel!.Note)
                .MaximumLength(255)
                .When(x => !string.IsNullOrEmpty(x.UpdateFundModel!.Note))
                .WithState(x =>
                    Messager
                        .Create<UpdateFundCommand>(nameof(Fund))
                        .Property(x => x.UpdateFundModel!.Note)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleFor(x => x.UpdateFundModel!.Status)
                .Must(command =>
                {
                    var role = _currentAccount.Session?.Role;
                    return role == "ADMIN" || role == "MANAGER";
                })
                .WithState(x =>
                    Messager
                        .Create<UpdateFundCommand>(nameof(Fund))
                        .Property(x => x.UpdateFundModel!.Status)
                        .Message(MessageType.Valid)
                        .Build()
                );
        }
    }
}
