using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Features.Accounts.Queries.List;

public class ListAccountHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListAccountQuery, PaginationResponse<ListAccountResponse>>
{
    public async ValueTask<PaginationResponse<ListAccountResponse>> Handle(
        ListAccountQuery query,
        CancellationToken cancellationToken
    ) =>
        await unitOfWork
            .Repository<Account>()
            .PagedListAsync<ListAccountResponse>(
                new ListAccountSpecification(),
                query.ValidateQuery().ValidateFilter(typeof(ListAccountResponse))
            );
}
