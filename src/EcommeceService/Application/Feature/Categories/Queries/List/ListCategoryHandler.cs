using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.QueryStringProcessing;
using Application.Feature.Common.Projections.Categories;
using AutoMapper;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using Infrastructure.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Categories.Queries.List;

public class ListCategoryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListCategoryQuery, IEnumerable<ListCategoryResponse>>
{
    public async ValueTask<IEnumerable<ListCategoryResponse>> Handle(
        ListCategoryQuery query,
        CancellationToken cancellationToken
    ) =>
        await unitOfWork
            .CachedRepository<Category>()
            .ListAsync<ListCategoryResponse>(cancellationToken);
}
