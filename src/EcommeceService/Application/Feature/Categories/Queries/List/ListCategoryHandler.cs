using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using Mediator;

namespace Application.Feature.Categories.Queries.List;

public class ListCategoryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListCategoryQuery, Result<PaginationResponse<ListCategoryResponse>>>
{
    public async ValueTask<Result<PaginationResponse<ListCategoryResponse>>> Handle(
        ListCategoryQuery query,
        CancellationToken cancellationToken
    )
    {
        var validation = query.Validate<ListCategoryQuery, ListCategoryResponse>();

        if (validation != null)
        {
            return validation;
        }
        var response = await unitOfWork
            .DynamicReadOnlyRepository<Category>()
            .PagedListAsync(
                new ListCategorySpecification(),
                query,
                ListCategoryMapping.Selector(),
                cancellationToken
            );

        return Result<PaginationResponse<ListCategoryResponse>>.Success(response);
    }
}
