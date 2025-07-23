using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using FluentValidation;

namespace Application.Features.Customers.Command.Update;

public partial class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator(IActionAccessorService accessorService)
    {
        _ = long.TryParse(accessorService.Id, out long id);

        RuleFor(x => x.Account)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<UpdateCustomerCommand>()
                    .Property(x => x.Account!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .ChildRules(item =>
            {
                item.RuleFor(x => x.DisplayName)
                    .MaximumLength(256)
                    .WithState(x =>
                        Messager
                            .Create<Account>()
                            .Property(x => x.DisplayName)
                            .Message(MessageType.MaximumLength)
                            .Build()
                    );

                item.RuleFor(x => x.Status)
                    .IsInEnum()
                    .WithState(x =>
                        Messager
                            .Create<Account>()
                            .Property(x => x.DisplayName)
                            .Message(MessageType.OuttaOption)
                            .Build()
                    );

                item.RuleFor(x => x.PhoneNumber)
                    .NotEmpty()
                    .WithState(x =>
                        Messager
                            .Create<Account>()
                            .Property(x => x.PhoneNumber)
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
                            .Create<Account>()
                            .Property(x => x.PhoneNumber)
                            .Message(MessageType.Valid)
                            .Negative()
                            .Build()
                    );
            });
    }

    [GeneratedRegex(@"^\+?\d{7,15}$")]
    private static partial Regex PhoneValidationRegex();
}
