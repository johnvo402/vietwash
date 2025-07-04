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
    private readonly IActionAccessorService accessorService;

    public CreateAccountCommandValidator(
        IUnitOfWork unitOfWork,
        IActionAccessorService accessorService
    )
    {
        this.unitOfWork = unitOfWork;
        this.accessorService = accessorService;

        ApplyRules();
    }

    private void ApplyRules()
    {
        Include(new AccountValidator(accessorService));
        _ = long.TryParse(accessorService.Id, out long id);
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
                (email, cancellationToken) => IsEmailAvailableAsync(email!, id, cancellationToken)
            )
            .When(
                _ => accessorService.GetHttpMethod() == HttpMethod.Put.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(x =>
                Messager
                    .Create<Account>()
                    .Property(x => x.Email)
                    .Message(MessageType.Existence)
                    .Build()
            )
            .MustAsync(
                (email, cancellationToken) =>
                    IsEmailAvailableAsync(email!, cancellationToken: cancellationToken)
            )
            .When(
                _ => accessorService.GetHttpMethod() == HttpMethod.Post.ToString(),
                ApplyConditionTo.CurrentValidator
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

    private async Task<bool> IsRolesAvailableAsync(string roles)
    {
        return new List<string> { "ADMIN", "MANAGER", "STAFF", "CUSTOMER" }.Contains(roles);
    }

    private async Task<bool> IsEmailAvailableAsync(
        string email,
        long? id = null,
        CancellationToken cancellationToken = default
    ) =>
        !await unitOfWork
            .Repository<Account>()
            .AnyAsync(
                x =>
                    (!id.HasValue && EF.Functions.ILike(x.Email, email))
                    || (x.Id != id && EF.Functions.ILike(x.Email, email)),
                cancellationToken
            );

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailValidationRegex();

    [GeneratedRegex(@"^((?=\S*?[A-Z])(?=\S*?[a-z])(?=\S*?[0-9]).{8,})\S$")]
    private static partial Regex PasswordValidationRegex();
}
