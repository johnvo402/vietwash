using Application.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;

namespace Application.Features.Users.Commands.Delete;

public class DeleteUserHandler(IUnitOfWork unitOfWork, IMediaUpdateService mediaUpdateService)
    : IRequestHandler<DeleteUserCommand, Result<MessageOutput>>
{
    public async ValueTask<Result<MessageOutput>> Handle(
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
        if (user == null)
        {
            return Result<MessageOutput>.Failure(
                new NotFoundError(
                    "Your resource is not found",
                    Messager.Create<User>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }
        string? oldAvatar = user.AvtUrl;
        user.Disabled = true;
        try
        {
            _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

            await unitOfWork.Repository<User>().UpdateAsync(user);
            await unitOfWork.SaveAsync(cancellationToken);

            await unitOfWork.Repository<User>().UpdateAsync(user);
            await unitOfWork.CommitAsync(cancellationToken);

            await mediaUpdateService.DeleteAvatarAsync(oldAvatar);
            return Result<MessageOutput>.Success(new MessageOutput { Message = "Success" });
        }
        catch (Exception)
        {
            await mediaUpdateService.DeleteAvatarAsync(user.AvtUrl);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
