using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Users;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator(
        IUnitOfWork unitOfWork,
        IActionAccessorService accessorService
    )
    {
        _ = Ulid.TryParse(accessorService.Id, out Ulid id);

        RuleFor(x => x.User)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<UpdateUserCommand>()
                    .Property(x => x.User!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .SetValidator(new UserValidator(unitOfWork, accessorService)!);

        RuleFor(x => x.User!.RoleId)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<UpdateUser>(nameof(User))
                    .Property(x => x.RoleId!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );
    }
}
