using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Features.Users.Queries.List;

public class ListUserHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListUserQuery, PaginationResponse<ListUserResponse>>
{
    public async ValueTask<PaginationResponse<ListUserResponse>> Handle(
        ListUserQuery query,
        CancellationToken cancellationToken
    ) =>
        await unitOfWork
            .Repository<User>()
            .PagedListAsync<ListUserResponse>(
                new ListUserSpecification(),
                query.ValidateQuery().ValidateFilter(typeof(ListUserResponse))
            );
}
