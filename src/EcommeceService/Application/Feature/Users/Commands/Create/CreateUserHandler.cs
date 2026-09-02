using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Application.Features.Users.Commands.Create;

public class CreateUserHandler(IUnitOfWork unitOfWork, IMediaUpdateService mediaUpdateService)
    : IRequestHandler<CreateAccountCommand, PubSubResponse<CreateAccountCommand>>
{
    public const string UserPrimaryKeyConstraint = "pk_user";

    public async ValueTask<PubSubResponse<CreateAccountCommand>> Handle(
        CreateAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        User mappingUser = command.Payload!.ToUser();

        string? userAvatar = null;
        try
        {
            IAsyncRepository<User> users = unitOfWork.Repository<User>();
            if (await users.AnyAsync(user => user.Id == mappingUser.Id, cancellationToken))
                return Success(command);

            _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

            User user = await users.AddAsync(mappingUser, cancellationToken);
            userAvatar = user.AvtUrl;

            await unitOfWork.SaveAsync(cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);

            return Success(command);
        }
        catch (DbUpdateException ex) when (IsDuplicateUserPrimaryKey(ex))
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            return Success(command);
        }
        catch (Exception ex)
        {
            await mediaUpdateService.DeleteMediaAsync(userAvatar);
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

    public static bool IsDuplicateUserPrimaryKey(DbUpdateException exception) =>
        exception.InnerException
            is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                TableName: "user",
                ConstraintName: UserPrimaryKeyConstraint,
            };

    private static PubSubResponse<CreateAccountCommand> Success(CreateAccountCommand command) =>
        new()
        {
            Error = null,
            ErrorType = null,
            IsSuccess = true,
            ResponseData = command,
            LastAttemptTime = DateTimeOffset.UtcNow,
            PayloadId = command.PayloadId,
        };
}
