using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Infrastructure.Constants;
using Mediator;

namespace Application.Features.BranchAccounts;

public class BranchAccountCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<BranchAccountCommand, PubSubResponse<BranchAccountCommand>>
{
    public async ValueTask<PubSubResponse<BranchAccountCommand>> Handle(
        BranchAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        var response = await unitOfWork
            .DynamicReadOnlyRepository<Account>()
            .ListAsync(
                new ListAccountSpecification([ROLE.ADMIN]),
                new QueryParamRequest(),
                cancellationToken: cancellationToken
            );

        if (response is null || !response.Any())
        {
            return SuccessResponse(command);
        }
        List<BranchAccount> branchAccounts = new List<BranchAccount>();
        foreach (var account in response)
        {
            branchAccounts.Add(
                new BranchAccount
                {
                    AccountId = account.Id,
                    BranchName = command.Payload.Name,
                    BranchId = command.Payload.BranchId,
                }
            );
        }

        try
        {
            _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
            await unitOfWork
                .Repository<BranchAccount>()
                .AddRangeAsync(branchAccounts, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            return new PubSubResponse<BranchAccountCommand>
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

    private static PubSubResponse<BranchAccountCommand> SuccessResponse(
        BranchAccountCommand command
    ) =>
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
