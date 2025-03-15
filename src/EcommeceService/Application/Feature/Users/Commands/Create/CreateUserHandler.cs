using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users;
using Mediator;

namespace Application.Features.Users.Commands.Create;

public class CreateUserHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediaUpdateService<User> mediaUpdateService
) : IRequestHandler<CreateUserCommand, QueueResponse<CreateUserCommand>>
{
    public async ValueTask<QueueResponse<CreateUserCommand>> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        User mappingUser = mapper.Map<User>(command.Payload);

        string? userAvatar = null;
        try
        {

            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            User user = await unitOfWork
                .Repository<User>()
                .AddAsync(mappingUser, cancellationToken);
            userAvatar = user.Avatar;

            await unitOfWork.SaveAsync(cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);
            return new QueueResponse<CreateUserCommand>
            {
                Error = "lỗi",
                ErrorType = Contracts.Dtos.Responses.QueueErrorType.Transient,
                IsSuccess = false,
                ResponseData = command,
                LastAttemptTime = DateTime.UtcNow,
                PayloadId = command.PayloadId,
            };

            return new QueueResponse<CreateUserCommand>
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
            return new QueueResponse<CreateUserCommand>
            {
                Error = ex.Message,
                ErrorType = Contracts.Dtos.Responses.QueueErrorType.Transient,
                IsSuccess = false,
                ResponseData = command,
                LastAttemptTime = DateTime.UtcNow,
                PayloadId = command.PayloadId,
            };
        }
    }
}
