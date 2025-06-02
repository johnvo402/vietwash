using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Queries.Detail;

public class GetAccountDetailHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAccountDetailQuery, GetAccountDetailResponse>
{
    public async ValueTask<GetAccountDetailResponse> Handle(
        GetAccountDetailQuery query,
        CancellationToken cancellationToken
    ) =>
        await unitOfWork
            .Repository<Account>()
            .FindByConditionAsync<GetAccountDetailResponse>(
                new GetAccountByIdSpecification(query.AccountId),
                cancellationToken
            )
        ?? throw new NotFoundException(
            [Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()]
        );
}
