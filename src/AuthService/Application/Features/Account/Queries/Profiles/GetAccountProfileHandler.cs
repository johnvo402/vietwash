using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Queries.Profiles;

public class GetAccountProfileHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
    : IRequestHandler<GetAccountProfileQuery, GetAccountProfileResponse>
{
    public async ValueTask<GetAccountProfileResponse> Handle(
        GetAccountProfileQuery query,
        CancellationToken cancellationToken
    ) =>
        await unitOfWork
            .Repository<Account>()
            .FindByConditionAsync<GetAccountProfileResponse>(
                new GetAccountByIdSpecification(currentAccount.Id!.Value),
                cancellationToken
            )
        ?? throw new NotFoundException(
            [Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()]
        );
}
