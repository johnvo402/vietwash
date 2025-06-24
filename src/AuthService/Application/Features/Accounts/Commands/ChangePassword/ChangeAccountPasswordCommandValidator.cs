using System.Text.RegularExpressions;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using FluentValidation;

namespace Application.Features.Accounts.Commands.ChangePassword;

public partial class ChangeAccountPasswordCommandValidator
    : AbstractValidator<ChangeAccountPasswordCommand>
{
    public ChangeAccountPasswordCommandValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<ChangeAccountPasswordCommand>(nameof(Account))
                    .Property(x => x.OldPassword!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<ChangeAccountPasswordCommand>(nameof(Account))
                    .Property(x => x.NewPassword!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .Must(x =>
            {
                Regex regex = PassowordValidationRegex();
                return regex.IsMatch(x!);
            })
            .WithState(x =>
                Messager
                    .Create<ChangeAccountPasswordCommand>(nameof(Account))
                    .Property(x => x.NewPassword!)
                    .Message(MessageType.Strong)
                    .Negative()
                    .Build()
            );
    }

    [GeneratedRegex(@"^((?=\S*?[A-Z])(?=\S*?[a-z])(?=\S*?[0-9]).{8,})\S$")]
    private static partial Regex PassowordValidationRegex();
}
