using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Services.Specifications;
using Mediator;
using Unit = Domain.Aggregates.Services.Unit;

namespace Application.Feature.Units.Queries.List
{
    public class ListUnitHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ListUnitQuery, Result<PaginationResponse<ListUnitResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<ListUnitResponse>>> Handle(
            ListUnitQuery query,
            CancellationToken cancellationToken
        )
        {
            var validation = query.Validate<ListUnitQuery, ListUnitResponse>();

            if (validation != null)
            {
                return validation;
            }
            var response = await unitOfWork
                .DynamicReadOnlyRepository<Unit>()
                .PagedListAsync(
                    new ListUnitSpecification(),
                    query,
                    ListUnitMapping.Selector(),
                    cancellationToken: cancellationToken
                );
            return Result<PaginationResponse<ListUnitResponse>>.Success(response);
        }
    }
}
