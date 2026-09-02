using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Application.Features.Users.Commands.Create;

public class CreateUserHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateUserCommand, PubSubResponse<CreateUserCommand>>
{
    public const string UserPrimaryKeyConstraint = "pk_user";

    public async ValueTask<PubSubResponse<CreateUserCommand>> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        User mappingUser = command.Payload!.ToUser();

        try
        {
            IAsyncRepository<User> users = unitOfWork.Repository<User>();
            if (await users.AnyAsync(user => user.Id == mappingUser.Id, cancellationToken))
                return Success(command);

            _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

            _ = await users.AddAsync(mappingUser, cancellationToken);

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
            await unitOfWork.RollbackAsync(cancellationToken);
            return new PubSubResponse<CreateUserCommand>
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

    private static PubSubResponse<CreateUserCommand> Success(CreateUserCommand command) =>
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
