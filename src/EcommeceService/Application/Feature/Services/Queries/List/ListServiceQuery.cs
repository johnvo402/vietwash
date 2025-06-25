using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Services.Queries.List;

public class ListServiceQuery
    : QueryParamRequest,
        IRequest<Result<PaginationResponse<ListServiceResponse>>>;
