using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.QueryStringProcessing;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Services.Queries.List;

public class ListServiceHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListServiceQuery, PaginationResponse<ListServiceResponse>>
{
    public async ValueTask<PaginationResponse<ListServiceResponse>> Handle(
        ListServiceQuery query,
        CancellationToken cancellationToken
    )
    {
        try
        {
           return await unitOfWork.Repository<Service>().
       PagedListAsync<ListServiceResponse>(
    new ListServiceSpecification(),
    query.ValidateQuery().ValidateFilter(typeof(ListServiceResponse))

);
        }
        catch(Exception ex)
        {
            throw new Exception("Exception", ex) ;
        }
    }

}
