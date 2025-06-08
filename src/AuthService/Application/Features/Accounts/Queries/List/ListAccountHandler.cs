using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.QueryStringProcessing;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Application.Common.Interfaces.Services.DistributedCache;
using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using JohnChum.SharedKernel.Extensions;
using Application.Features.Common.Helpers;

namespace Application.Features.Accounts.Queries.List;

public class ListAccountHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
    : IRequestHandler<ListAccountQuery, PaginationResponse<ListAccountResponse>>
{
    public async ValueTask<PaginationResponse<ListAccountResponse>> Handle(
        ListAccountQuery query,
        CancellationToken cancellationToken
    )
    {
       
        string[] roles = AccountHelper.GetRolesByRole(currentAccount.Session!.Role!);
        return await unitOfWork
            .Repository<Account>()
            .PagedListAsync<ListAccountResponse>(
                new ListAccountSpecification(roles),
                query.ValidateQuery().ValidateFilter(typeof(ListAccountResponse))
            );
    }
       
}
