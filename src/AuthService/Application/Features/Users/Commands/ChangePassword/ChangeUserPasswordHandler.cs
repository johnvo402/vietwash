using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;

namespace Application.Features.Users.Commands.ChangePassword;

public class ChangeUserPasswordHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
   : IRequestHandler<ChangeUserPasswordCommand>
{
    public async ValueTask<Unit> Handle(
        ChangeUserPasswordCommand request,
        CancellationToken cancellationToken
    )
    {
        long? userId = currentUser.Id;

        if (!userId.HasValue)
        {
            throw new NotFoundException(
                [Messager.Create<User>().Message(MessageType.Found).Negative().Build()]
            );
        }

        User user =
            await unitOfWork
                .Repository<User>()
                .FindByConditionAsync(
                    new GetUserByIdWithoutIncludeSpecification(userId.Value),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<User>().Message(MessageType.Found).Negative().Build()]
            );

        if (!Verify(request.OldPassword, user.Password))
        {
            throw new BadRequestException(
                [
                    Messager
                       .Create<ChangeUserPasswordCommand>(nameof(User))
                       .Property(x => x.OldPassword!)
                       .Message(MessageType.Correct)
                       .Negative()
                       .Build(),
               ]
            );
        }

        user.SetPassword(HashPassword(request.NewPassword));

        await unitOfWork.Repository<User>().UpdateAsync(user);
        await unitOfWork.SaveAsync(cancellationToken);

        return Unit.Value;
    }
}
