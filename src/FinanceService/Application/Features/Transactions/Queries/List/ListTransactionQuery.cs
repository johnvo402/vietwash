using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Features.Transactions.Queries.List
{
    public class ListTransactionQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListTransactionResponse>>>;
}
