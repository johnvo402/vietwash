using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Domain.Aggregates.Users;
using Mediator;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserHandler(
    IUnitOfWork unitOfWork,
    IMediaUpdateService<Image> mediaUpdateService
) : IRequestHandler<UpdateUserCommand, Result<UpdateUserResponse>>
{
    public async ValueTask<Result<UpdateUserResponse>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await unitOfWork
            .Repository<User>()
            .FindByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result<UpdateUserResponse>.Failure(
                new NotFoundError(
                    "User not found",
                    Messager.Create<User>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }

        string? oldAvatar = user.AvtUrl;

        user.FromUpdateModel(model: request.User!);
        try
        {
            _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

            await unitOfWork.Repository<User>().UpdateAsync(user);
            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            // 🔁 Nếu avatar đã đổi thì xoá ảnh cũ
            if (
                !string.IsNullOrWhiteSpace(oldAvatar)
                && oldAvatar != user.AvtUrl
                && !string.IsNullOrWhiteSpace(user.AvtUrl)
            )
            {
                await mediaUpdateService.DeleteAvatarAsync(oldAvatar);
            }

            return Result<UpdateUserResponse>.Success(user.ToUpdateUserResponse());
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync(cancellationToken);

            // Nếu user.AvtUrl là mới upload, xoá ảnh rác
            if (!string.IsNullOrWhiteSpace(user.AvtUrl))
            {
                await mediaUpdateService.DeleteAvatarAsync(user.AvtUrl);
            }

            throw;
        }
    }
}
