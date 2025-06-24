using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Queries.Detail;

public class GetAccountDetailHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAccountDetailQuery, Result<GetAccountDetailResponse>>
{
    public async ValueTask<Result<GetAccountDetailResponse>> Handle(
        GetAccountDetailQuery query,
        CancellationToken cancellationToken
    )
    {
        GetAccountDetailResponse? account = await unitOfWork
            .DynamicReadOnlyRepository<Account>()
            .FindByConditionAsync(
                new GetAccountByIdSpecification(query.AccountId),
                x => x.ToGetAccountDetailResponse(),
                cancellationToken
            );
        if (account == null)
        {
            return Result<GetAccountDetailResponse>.Failure(
                new NotFoundError(
                    "Account not found",
                    Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }
        return Result<GetAccountDetailResponse>.Success(account);
    }
}
