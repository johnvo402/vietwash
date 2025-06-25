using System.Text.RegularExpressions;
using Contracts.Common.Messages;
using FluentValidation;

namespace Application.Features.Accounts.Commands.Login;

public partial class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        ApplyRules();
    }

    private void ApplyRules()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<LoginCommand>()
                    .Property(x => x.Email!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .Must(x =>
            {
                Regex regex = EmailValidationRegex();
                return regex.IsMatch(x!);
            })
            .WithState(x =>
                Messager
                    .Create<LoginCommand>()
                    .Property(x => x.Email!)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build()
            );
    }

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailValidationRegex();
}
