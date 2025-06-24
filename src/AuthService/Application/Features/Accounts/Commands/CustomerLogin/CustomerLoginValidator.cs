using System.Text.RegularExpressions;
using Contracts.Common.Messages;
using FluentValidation;

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
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CustomerLoginCommand>()
                    .Property(x => x.PhoneNumber!)
                    .Message(MessageType.Empty)
                    .Negative()
                    .Build()
            );
    }
}
