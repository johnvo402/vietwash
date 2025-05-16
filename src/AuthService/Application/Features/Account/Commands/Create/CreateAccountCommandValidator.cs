using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Accounts;
using Domain.Aggregates.Accounts;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
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
        Include(new AccountValidator(unitOfWork, accessorService));

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

    [GeneratedRegex(@"^((?=\S*?[A-Z])(?=\S*?[a-z])(?=\S*?[0-9]).{8,})\S$")]
    private static partial Regex PasswordValidationRegex();
}
