using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;

namespace Application.Features.Users.Queries.List;

public class ListUserHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListUserQuery, Result<PaginationResponse<ListUserResponse>>>
{
    public async ValueTask<Result<PaginationResponse<ListUserResponse>>> Handle(
        ListUserQuery query,
        CancellationToken cancellationToken
    )
    {
        var validation = query.Validate<ListUserQuery, ListUserResponse>();

        if (validation != null)
        {
            return validation;
        }
        var response = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .PagedListAsync<ListUserResponse>(
                new ListUserSpecification(),
                query,
                ListUserMapping.Selector(),
                cancellationToken
            );
        return Result<PaginationResponse<ListUserResponse>>.Success(response);
    }
}
