using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Common.Messages;
using Domain.Aggregates.Users;
using FluentValidation;
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

        RuleFor(x => x.Payload!.Role)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CreateUserCommand>(nameof(User))
                    .Property(x => x.Payload!.Role)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );
    }
}
