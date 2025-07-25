using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Vouchers.Queries.List
{
    public class ListVoucherQuery : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListVoucherResponse>>>;

}
