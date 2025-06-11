using Domain.Otp;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;

namespace Application.Features.Accounts.Commands.VerifyOtpLoginCustomer
{
    public class VerifyOtpValidation : AbstractValidator<VerifyOtpCommand>
    {
        public VerifyOtpValidation()
        {
            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.Otp)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<VerifyPinRequest>()
                        .Property(x => x.Otp!)
                        .Message(MessageType.Empty)
                        .Negative()
                        .Build()
                );
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<VerifyOtpCommand>()
                        .Property(x => x.PhoneNumber!)
                        .Message(MessageType.Empty)
                        .Negative()
                        .Build()
                );
            RuleFor(x => x.Type)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<VerifyPinRequest>()
                        .Property(x => x.Type!)
                        .Message(MessageType.Empty)
                        .Negative()
                        .Build()
                );
        }
    }
}
