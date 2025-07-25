using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Services.Queries.GetByTariff
{
    public class GetByTariffQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<GetByTariffResponse>>>
    {
        public long TariffId { get; set; }
    };
}
