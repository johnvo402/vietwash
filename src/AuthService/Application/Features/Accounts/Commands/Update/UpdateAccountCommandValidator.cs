using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Accounts;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.Update;

public partial class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IActionAccessorService accessorService;

    public UpdateAccountCommandValidator(
        IActionAccessorService accessorService,
        IUnitOfWork unitOfWork
    )
    {
        this.unitOfWork = unitOfWork;
        this.accessorService = accessorService;
    }

    private void ApplyRule()
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
            .SetValidator(new AccountValidator()!);

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
        RuleFor(x => x.Account!.Email)
            .NotEmpty()
            .When(x => x.Account!.Role != "CUSTOMER")
            .WithState(x =>
                Messager
                    .Create<Account>()
                    .Property(x => x.Email)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .Must(x =>
            {
                Regex regex = EmailValidationRegex();
                return regex.IsMatch(x!);
            })
            .When(x => x.Account!.Role != "CUSTOMER")
            .WithState(x =>
                Messager
                    .Create<Account>()
                    .Property(x => x.Email)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build()
            )
            .MustAsync(
                (email, cancellationToken) => IsEmailAvailableAsync(email!, id, cancellationToken)
            )
            .When(x => x.Account!.Role != "CUSTOMER")
            .WithState(x =>
                Messager
                    .Create<Account>()
                    .Property(x => x.Email)
                    .Message(MessageType.Existence)
                    .Build()
            );
    }

    private async Task<bool> IsEmailAvailableAsync(
        string email,
        long id,
        CancellationToken cancellationToken = default
    ) =>
        !await unitOfWork
            .Repository<Account>()
            .AnyAsync(
                x => x.Id != id && x.Email != null && EF.Functions.ILike(x.Email, email),
                cancellationToken
            );

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailValidationRegex();
}
