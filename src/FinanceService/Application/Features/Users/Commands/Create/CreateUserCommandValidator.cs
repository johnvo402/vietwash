using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Users;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands.Create;

public partial class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IActionAccessorService accessorService;

    public CreateUserCommandValidator(
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
        RuleFor(x => x.Payload!.Username)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Payload!.Username!)
                    .Message(MessageType.Null)
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

        RuleFor(x => x.Payload!.Gender)
            .IsInEnum()
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Payload!.Gender!)
                    .Message(MessageType.OuttaOption)
                    .Build()
            );

        RuleFor(x => x.Payload!.Status)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Payload!.Status!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );

        RuleFor(x => x.Payload!.RoleId)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Payload!.RoleId)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );
    }

    private async Task<bool> IsUsernameAvailableAsync(
        string username,
        long? id = null,
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
}
