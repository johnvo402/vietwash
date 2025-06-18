using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Users;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands.Create;

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
        RuleFor(x => x.Payload!.Gender)
            .IsInEnum()
            .WithState(x =>
                Messager
                    .Create<CreateAccountCommand>(nameof(User))
                    .Property(x => x.Payload!.Gender!)
                    .Message(MessageType.OuttaOption)
                    .Build()
            );

        RuleFor(x => x.Payload!.Status)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CreateAccountCommand>(nameof(User))
                    .Property(x => x.Payload!.Status!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );

        RuleFor(x => x.Payload!.Role)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<CreateAccountCommand>(nameof(User))
                    .Property(x => x.Payload!.Role)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );
    }
}
