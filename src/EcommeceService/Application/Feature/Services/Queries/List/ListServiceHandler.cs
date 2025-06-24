using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using Mediator;

namespace Application.Feature.Services.Queries.List;

public class ListServiceHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListServiceQuery, Result<PaginationResponse<ListServiceResponse>>>
{
    public async ValueTask<Result<PaginationResponse<ListServiceResponse>>> Handle(
        ListServiceQuery query,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var validation = query.Validate<ListServiceQuery, ListServiceResponse>();

            if (validation != null)
            {
                return validation;
            }

            var response = await unitOfWork
                .DynamicReadOnlyRepository<Service>()
                .PagedListAsync(
                    new ListServiceSpecification(),
                    query,
                    ListServiceMapping.Selector(),
                    cancellationToken
                );

            return Result<PaginationResponse<ListServiceResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            throw new Exception("Exception", ex);
        }
    }
}
