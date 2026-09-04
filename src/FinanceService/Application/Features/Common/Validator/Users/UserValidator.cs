using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.Users;
using Contracts.Common.Messages;
using Domain.Aggregates.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Common.Validators.Users;

public partial class UserValidator : AbstractValidator<UserModel>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IActionAccessorService accessorService;

    public UserValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
    {
        this.unitOfWork = unitOfWork;
        this.accessorService = accessorService;
        ApplyRules();
    }

    private void ApplyRules()
    {
        _ = long.TryParse(accessorService.Id, out long id);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<User>()
                    .Property(x => x.DisplayName)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .MaximumLength(256)
            .WithState(x =>
                Messager
                    .Create<User>()
                    .Property(x => x.DisplayName)
                    .Message(MessageType.MaximumLength)
                    .Build()
            );

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<User>()
                    .Property(x => x.Email)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .WithState(x =>
                Messager
                    .Create<User>()
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
                    .Create<User>()
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
                    .Create<User>()
                    .Property(x => x.Email)
                    .Message(MessageType.Existence)
                    .Build()
            );

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<User>()
                    .Property(x => x.PhoneNumber)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .WithState(x =>
                Messager
                    .Create<User>()
                    .Property(x => x.PhoneNumber)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build()
            );
    }

    private async Task<bool> IsEmailAvailableAsync(
        string email,
        long? id = null,
        CancellationToken cancellationToken = default
    ) =>
        !await unitOfWork
            .Repository<User>()
            .AnyAsync(
                x =>
                    x.Email != null
                    && (
                        (!id.HasValue && EF.Functions.ILike(x.Email, email))
                        || (x.Id != id && EF.Functions.ILike(x.Email, email))
                    ),
                cancellationToken
            );
}
