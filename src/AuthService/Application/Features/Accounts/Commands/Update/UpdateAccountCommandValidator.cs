using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Accounts;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using FluentValidation;

namespace Application.Features.Accounts.Commands.Update;

public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator(
        IUnitOfWork unitOfWork,
        IActionAccessorService accessorService
    )
    {
        _ = long.TryParse(accessorService.Id, out long id);

        RuleFor(x => x.Account)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<UpdateAccountCommand>()
                    .Property(x => x.Account!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .SetValidator(new AccountValidator(unitOfWork, accessorService)!);

        RuleFor(x => x.Account!.Role)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<UpdateAccount>(nameof(Account))
                    .Property(x => x.Role!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );
    }
}
