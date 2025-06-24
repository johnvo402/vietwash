using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.Users;
using Application.Features.Common.Validators.Users;
using Contracts.Common.Messages;
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

        RuleFor(x => x.User!.Role)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<UserModel>(nameof(User))
                    .Property(x => x.Role!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );
    }
}
