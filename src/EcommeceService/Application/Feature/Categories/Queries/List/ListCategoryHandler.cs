using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Categories.Queries.List;

public class ListCategoryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListCategoryQuery, PaginationResponse<ListCategoryResponse>>
{
    public async ValueTask<PaginationResponse<ListCategoryResponse>> Handle(
        ListCategoryQuery query,
        CancellationToken cancellationToken
    ) =>
        await unitOfWork
            .Repository<Category>()
            .PagedListAsync<ListCategoryResponse>(new ListCategorySpecification(), query, cancellationToken);
}
