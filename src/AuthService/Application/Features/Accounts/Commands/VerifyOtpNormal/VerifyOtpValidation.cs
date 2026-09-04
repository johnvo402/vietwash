using System.Text.RegularExpressions;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Otp;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.VerifyOtpNormal
{
    public partial class VerifyOtpNormalValidation : AbstractValidator<VerifyOtpNormalCommand>
    {
        private readonly IUnitOfWork unitOfWork;

        public VerifyOtpNormalValidation(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

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
            RuleFor(x => x.Email)
                .Must(x =>
                {
                    Regex regex = EmailValidationRegex();
                    return regex.IsMatch(x!);
                })
                .When(x => !string.IsNullOrEmpty(x.Email))
                .WithState(x =>
                    Messager
                        .Create<VerifyOtpNormalCommand>(nameof(Account))
                        .Property(x => x.Email)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build()
                )
                .MustAsync(
                    (email, cancellationToken) => IsEmailAvailableAsync(email!, cancellationToken)
                )
                .When(x => !string.IsNullOrEmpty(x.Email))
                .WithState(x =>
                    Messager
                        .Create<Account>()
                        .Property(x => x.Email)
                        .Message(MessageType.Existence)
                        .Build()
                );

            RuleFor(x => x.PhoneNumber)
                .Must(x =>
                {
                    Regex regex = PhoneValidationRegex();
                    return regex.IsMatch(x!);
                })
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithState(x =>
                    Messager
                        .Create<Account>()
                        .Property(x => x.PhoneNumber)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build()
                );
        }

        private async Task<bool> IsEmailAvailableAsync(
            string email,
            CancellationToken cancellationToken = default
        ) =>
            !await unitOfWork
                .Repository<Account>()
                .AnyAsync(
                    x => x.Email != null && EF.Functions.ILike(x.Email, email),
                    cancellationToken
                );

        [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
        private static partial Regex EmailValidationRegex();

        [GeneratedRegex(@"^\+?\d{7,15}$")]
        private static partial Regex PhoneValidationRegex();
    }
}
