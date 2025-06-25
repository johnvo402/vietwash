using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Features.Accounts.Queries.List;

public class ListAccountQuery
    : QueryParamRequest,
        IRequest<Result<PaginationResponse<ListAccountResponse>>>;
