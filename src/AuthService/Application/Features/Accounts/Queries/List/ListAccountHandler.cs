using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Helpers;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Queries.List;

public class ListAccountHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
    : IRequestHandler<ListAccountQuery, Result<PaginationResponse<ListAccountResponse>>>
{
    public async ValueTask<Result<PaginationResponse<ListAccountResponse>>> Handle(
        ListAccountQuery query,
        CancellationToken cancellationToken
    )
    {
        var validation = query.Validate<ListAccountQuery, ListAccountResponse>();

        if (validation != null)
        {
            return validation;
        }
        string[] roles = AccountHelper.GetRolesByRole(currentAccount.Session!.Role!);
        var response = await unitOfWork
            .DynamicReadOnlyRepository<Account>()
            .PagedListAsync(
                new ListAccountSpecification(roles),
                query,
                ListAccountMapping.Selector(),
                cancellationToken: cancellationToken
            );
        return Result<PaginationResponse<ListAccountResponse>>.Success(response);
    }
}
