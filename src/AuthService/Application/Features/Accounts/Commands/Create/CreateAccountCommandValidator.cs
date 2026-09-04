using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Accounts;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.Create;

public partial class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    private readonly IUnitOfWork unitOfWork;

    public CreateAccountCommandValidator(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
        ApplyRules();
    }

    private void ApplyRules()
    {
        Include(new AccountValidator());
        RuleFor(x => x.Email)
            .NotEmpty()
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
            .WithState(x =>
                Messager
                    .Create<Account>()
                    .Property(x => x.Email)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build()
            )
            .MustAsync(
                (email, cancellationToken) => IsEmailAvailableAsync(email!, cancellationToken)
            )
            .WithState(x =>
                Messager
                    .Create<Account>()
                    .Property(x => x.Email)
                    .Message(MessageType.Existence)
                    .Build()
            );

        RuleFor(x => x.Password)
            .Must(
                (_, x) =>
                {
                    Regex regex = PasswordValidationRegex();
                    return regex.IsMatch(x!) && !string.IsNullOrEmpty(x);
                }
            )
            .WithState(x =>
                Messager
                    .Create<CreateAccountCommand>(nameof(Account))
                    .Property(x => x.Password!)
                    .Message(MessageType.Strong)
                    .Negative()
                    .Build()
            );

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithState(x =>
                Messager
                    .Create<CreateAccountCommand>(nameof(Account))
                    .Property(x => x.Gender!)
                    .Message(MessageType.OuttaOption)
                    .Build()
            );

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CreateAccountCommand>(nameof(Account))
                    .Property(x => x.Status!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .IsInEnum()
            .WithState(x =>
                Messager
                    .Create<CreateAccountCommand>(nameof(Account))
                    .Property(x => x.Status!)
                    .Message(MessageType.OuttaOption)
                    .Build()
            );

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CreateAccountCommand>(nameof(Account))
                    .Property(x => x.Role)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .MustAsync((roles, cancellationToken) => IsRolesAvailableAsync(roles!))
            .WithState(x =>
                Messager
                    .Create<CreateAccountCommand>(nameof(Account))
                    .Property(x => x.Role)
                    .Message(MessageType.Found)
                    .Negative()
                    .Build()
            );
    }

    private Task<bool> IsRolesAvailableAsync(string roles) =>
        Task.FromResult(
            new List<string> { "ADMIN", "MANAGER", "STAFF", "CUSTOMER" }.Contains(roles)
        );

    private async Task<bool> IsEmailAvailableAsync(
        string email,
        CancellationToken cancellationToken = default
    ) =>
        !await unitOfWork
            .Repository<Account>()
            .AnyAsync(
                x => x.Email != null && EF.Functions.ILike(x.Email, email),
                cancellationToken
            );

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailValidationRegex();

    [GeneratedRegex(@"^((?=\S*?[A-Z])(?=\S*?[a-z])(?=\S*?[0-9]).{8,})\S$")]
    private static partial Regex PasswordValidationRegex();
}
