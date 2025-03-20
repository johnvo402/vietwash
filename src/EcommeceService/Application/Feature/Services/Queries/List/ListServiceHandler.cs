using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using Mediator;
using Application.Common.QueryStringProcessing;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;

namespace Application.Feature.Services.Queries.List;

public class ListServiceHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListServiceQuery, PaginationResponse<ListServiceResponse>>
{
    public async ValueTask<PaginationResponse<ListServiceResponse>> Handle(
        ListServiceQuery query,
        CancellationToken cancellationToken
    ) =>
        await unitOfWork
            .CachedRepository<Service>()
            .PagedListAsync<ListServiceResponse>(
                new ListServiceSpecification(),
                query.ValidateQuery().ValidateFilter(typeof(ListServiceResponse))
            );
}

