using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users;
using Mediator;

namespace Application.Features.Users.Commands.Create;

public class CreateUserHandler(IUnitOfWork unitOfWork, IMediaUpdateService<User> mediaUpdateService)
    : IRequestHandler<CreateAccountCommand, PubSubResponse<CreateAccountCommand>>
{
    public async ValueTask<PubSubResponse<CreateAccountCommand>> Handle(
        CreateAccountCommand command,
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

            return new PubSubResponse<CreateAccountCommand>
            {
                Error = null,
                ErrorType = null,
                IsSuccess = true,
                ResponseData = command,
                LastAttemptTime = DateTimeOffset.UtcNow,
                PayloadId = command.PayloadId,
            };
        }
        catch (Exception ex)
        {
            await mediaUpdateService.DeleteAvatarAsync(userAvatar);
            await unitOfWork.RollbackAsync(cancellationToken);
            return new PubSubResponse<CreateAccountCommand>
            {
                Error = ex.Message,
                ErrorType = PubSubErrorType.Transient,
                IsSuccess = false,
                ResponseData = command,
                LastAttemptTime = DateTime.UtcNow,
                PayloadId = command.PayloadId,
            };
        }
    }
}
