using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Tariffs.Queries
{
    public class ListTariffQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListTariffResponse>>>;
}
