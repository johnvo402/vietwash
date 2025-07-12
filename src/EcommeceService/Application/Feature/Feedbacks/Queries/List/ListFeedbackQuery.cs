using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Feedbacks.Queries.List;

public class ListFeedbackQuery
    : QueryParamRequest,
        IRequest<Result<PaginationResponse<ListFeedbackResponse>>>;
