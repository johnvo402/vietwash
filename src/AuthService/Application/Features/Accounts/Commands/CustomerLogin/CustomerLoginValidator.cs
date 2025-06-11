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
