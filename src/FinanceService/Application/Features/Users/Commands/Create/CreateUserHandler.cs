using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users;
using Mediator;

namespace Application.Features.Users.Commands.Create;

public class CreateUserHandler(IUnitOfWork unitOfWork, IMediaUpdateService mediaUpdateService)
    : IRequestHandler<CreateUserCommand, PubSubResponse<CreateUserCommand>>
{
    public async ValueTask<PubSubResponse<CreateUserCommand>> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        User mappingUser = command.Payload!.ToUser();

        string? userAvatar = null;
        try
        {
            DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            User user = await unitOfWork
                .Repository<User>()
                .AddAsync(mappingUser, cancellationToken);
            userAvatar = user.AvtUrl;

            await unitOfWork.SaveAsync(cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);

            return new PubSubResponse<CreateUserCommand>
            {
                Error = null,
                ErrorType = null,
                IsSuccess = true,
                ResponseData = command,
                LastAttemptTime = DateTime.UtcNow,
                PayloadId = command.PayloadId,
            };
        }
        catch (Exception ex)
        {
            await mediaUpdateService.DeleteAvatarAsync(userAvatar);
            await unitOfWork.RollbackAsync(cancellationToken);
            return new PubSubResponse<CreateUserCommand>
            {
                Error = ex.Message,
                ErrorType = Contracts.Dtos.Responses.PubSubErrorType.Transient,
                IsSuccess = false,
                ResponseData = command,
                LastAttemptTime = DateTime.UtcNow,
                PayloadId = command.PayloadId,
            };
        }
    }
}
