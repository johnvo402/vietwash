using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Users;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands.Create;

public partial class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IActionAccessorService accessorService;

    private readonly IRoleManagerService roleManagerService;

    public CreateUserCommandValidator(
        IUnitOfWork unitOfWork,
        IActionAccessorService accessorService,
        IRoleManagerService roleManagerService
    )
    {
        this.unitOfWork = unitOfWork;
        this.accessorService = accessorService;
        this.roleManagerService = roleManagerService;

        ApplyRules();
    }

    private void ApplyRules()
    {
        Include(new UserValidator(unitOfWork, accessorService));
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Username!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .Must(
                (_, x) =>
                {
                    Regex regex = UsernameValidationRegex();
                    return regex.IsMatch(x!);
                }
            )
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Username!)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build()
            )
            .MustAsync(
                (username, cancellationToken) =>
                    IsUsernameAvailableAsync(username!, cancellationToken: cancellationToken)
            )
            .WithState(x =>
                Messager
                    .Create<User>()
                    .Property(x => x.Username)
                    .Message(MessageType.Existence)
                    .Build()
            );

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Password!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .Must(
                (_, x) =>
                {
                    Regex regex = PassowordValidationRegex();
                    return regex.IsMatch(x!);
                }
            )
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Password!)
                    .Message(MessageType.Strong)
                    .Negative()
                    .Build()
            );

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Gender!)
                    .Message(MessageType.OuttaOption)
                    .Build()
            );

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Status!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .IsInEnum()
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Status!)
                    .Message(MessageType.OuttaOption)
                    .Build()
            );

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.RoleId)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .MustAsync((roles, cancellationToken) => IsRolesAvailableAsync(roles!))
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.RoleId)
                    .Message(MessageType.Found)
                    .Negative()
                    .Build()
            );

    }

    private async Task<bool> IsUsernameAvailableAsync(
        string username,
        Ulid? id = null,
        CancellationToken cancellationToken = default
    )
    {
        return !await unitOfWork
            .Repository<User>()
            .AnyAsync(
                x =>
                    (!id.HasValue && EF.Functions.ILike(x.Username, username))
                    || (x.Id != id && EF.Functions.ILike(x.Username, username)),
                cancellationToken
            );
    }

    private async Task<bool> IsRolesAvailableAsync(string roles)
    {
        return await roleManagerService.Roles.AnyAsync(x => Ulid.Parse(roles) == x.Id);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_.]+$")]
    private static partial Regex UsernameValidationRegex();

    [GeneratedRegex(@"^((?=\S*?[A-Z])(?=\S*?[a-z])(?=\S*?[0-9]).{8,})\S$")]
    private static partial Regex PassowordValidationRegex();
}
