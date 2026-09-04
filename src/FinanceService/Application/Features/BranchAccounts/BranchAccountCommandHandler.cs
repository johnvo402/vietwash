using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Infrastructure.Constants;
using Mediator;

namespace Application.Features.BranchUsers;

public class BranchUserCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<BranchUserCommand, PubSubResponse<BranchUserCommand>>
{
    public async ValueTask<PubSubResponse<BranchUserCommand>> Handle(
        BranchUserCommand command,
        CancellationToken cancellationToken
    )
    {
        if (command.Payload is null)
        {
            return new PubSubResponse<BranchUserCommand>
            {
                Error = "Payload is required.",
                ErrorType = PubSubErrorType.Persistent,
                IsSuccess = false,
                ResponseData = command,
                LastAttemptTime = DateTimeOffset.UtcNow,
                PayloadId = command.PayloadId,
            };
        }

        var response = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .ListAsync(
                new ListUserSpecification([ROLE.ADMIN, ROLE.CUSTOMER]),
                new QueryParamRequest(),
                cancellationToken: cancellationToken
            );

        if (response is null || !response.Any())
        {
            return SuccessResponse(command);
        }
        List<BranchUser> branchUsers = new List<BranchUser>();
        foreach (var account in response)
        {
            branchUsers.Add(
                new BranchUser
                {
                    UserId = account.Id,
                    BranchName = command.Payload.Name,
                    BranchId = command.Payload.BranchId,
                }
            );
        }

        try
        {
            _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
            await unitOfWork.Repository<BranchUser>().AddRangeAsync(branchUsers, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            return new PubSubResponse<BranchUserCommand>
            {
                Error = ex.Message,
                ErrorType = PubSubErrorType.Transient,
                IsSuccess = false,
                ResponseData = command,
                LastAttemptTime = DateTimeOffset.UtcNow,
                PayloadId = command.PayloadId,
            };
        }

        return SuccessResponse(command);
    }

    private static PubSubResponse<BranchUserCommand> SuccessResponse(BranchUserCommand command) =>
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
