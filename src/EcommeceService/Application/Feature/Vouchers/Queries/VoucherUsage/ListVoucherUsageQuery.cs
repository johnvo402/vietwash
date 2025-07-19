using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Vouchers.Queries.VoucherUsage
{
    public class ListVoucherUsageQuery : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListVoucherUsageResponse>>>;

}
