using System.Data;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Accounts;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using FluentValidation;

namespace Application.Features.Accounts.Commands.Profiles;

public class UpdateAccountProfileCommandValidator : AbstractValidator<UpdateAccountProfileCommand>
{
    public UpdateAccountProfileCommandValidator(IActionAccessorService accessorService)
    {
        Include(new AccountValidator(accessorService));

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithState(x =>
                Messager
                    .Create<Account>()
                    .Property(x => x.Email)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build()
            );

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithState(x =>
                Messager
                    .Create<Account>()
                    .Property(x => x.Gender)
                    .Message(MessageType.OuttaOption)
                    .Negative()
                    .Build()
            );
    }
}
