using System.Data;
using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Accounts;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.Profiles;

public class UpdateAccountProfileCommandValidator : AbstractValidator<UpdateAccountProfileCommand>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IActionAccessorService accessorService;
    private readonly ICurrentAccount currentAccount;

    public UpdateAccountProfileCommandValidator(
        IActionAccessorService accessorService,
        IUnitOfWork unitOfWork,
        ICurrentAccount currentAccount
    )
    {
        this.unitOfWork = unitOfWork;
        this.accessorService = accessorService;
        this.currentAccount = currentAccount;
        ApplyRule();
    }

    private void ApplyRule()
    {
        long? id = currentAccount.Id;
        Include(new AccountValidator());

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
            )
            .MustAsync(
                (email, cancellationToken) => IsEmailAvailableAsync(email, id, cancellationToken)
            )
            .When(
                (x, _) =>
                    accessorService.GetHttpMethod() == HttpMethod.Put.ToString()
                    && !string.IsNullOrEmpty(x.Email),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(x =>
                Messager
                    .Create<Account>()
                    .Property(x => x.Email)
                    .Message(MessageType.Existence)
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

    private async Task<bool> IsEmailAvailableAsync(
        string? email,
        long? id = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(email))
            return true;
        return !await unitOfWork
            .Repository<Account>()
            .AnyAsync(
                x =>
                    (!id.HasValue && EF.Functions.ILike(x.Email, email))
                    || (x.Id != id && EF.Functions.ILike(x.Email, email)),
                cancellationToken
            );
    }
}
