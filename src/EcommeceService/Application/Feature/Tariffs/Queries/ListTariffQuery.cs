using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Tariffs.Queries
{
    public class ListTariffQuery : QueryParamRequest, IRequest<PaginationResponse<ListTariffResponse>>
    {

    }
}