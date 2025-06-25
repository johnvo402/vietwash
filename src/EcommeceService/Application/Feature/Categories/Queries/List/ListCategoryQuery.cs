using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Categories.Queries.List;

public class ListCategoryQuery
    : QueryParamRequest,
        IRequest<Result<PaginationResponse<ListCategoryResponse>>>;
