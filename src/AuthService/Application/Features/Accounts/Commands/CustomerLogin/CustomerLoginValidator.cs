using System.Text.RegularExpressions;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;

namespace Application.Features.Accounts.Commands.CustomerLogin;

public partial class CustomerLoginValidator : AbstractValidator<CustomerLoginCommand>
{
    public CustomerLoginValidator()
    {
        ApplyRules();
    }

    private void ApplyRules()
    {
        RuleFor(x => x.PhoneNumber)
            .NotNull()
            .WithState(x =>
                Messager
                    .Create<CustomerLoginCommand>()
                    .Property(x => x.PhoneNumber!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .Must(x =>
            {
                Regex regex = PhoneValidationRegex();
                return regex.IsMatch(x!);
            })
            .WithState(x =>
                Messager
                    .Create<CustomerLoginCommand>()
                    .Property(x => x.PhoneNumber!)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build()
            );
    }

    [GeneratedRegex(@"^(0|\+84)(\d{9})$")]
    private static partial Regex PhoneValidationRegex();
}
