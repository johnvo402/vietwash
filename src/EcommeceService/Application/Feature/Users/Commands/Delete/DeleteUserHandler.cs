using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;

namespace Application.Features.Users.Commands.Delete;

public class DeleteUserHandler(
    IUnitOfWork unitOfWork,
    IMediaUpdateService MediaUpdateService
) : IRequestHandler<DeleteUserCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteUserCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(
                new GetUserByIdWithoutIncludeSpecification(command.UserId),
                cancellationToken
            );

        if (user is null)
        {
            return Result.Failure(
                new Application.Common.Errors.NotFoundError(
                    "User not found",
                    Messager.Create<User>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }

        string? avatar = user.AvtUrl;
        await unitOfWork.Repository<User>().DeleteAsync(user);
        await unitOfWork.SaveAsync(cancellationToken);

        await MediaUpdateService.DeleteMediaAsync(avatar);
        return Result.Success();
    }
}
