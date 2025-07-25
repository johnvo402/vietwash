
using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Features.Notifications.Queries.List
{
    public class ListNotificationQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListNotificationResponse>>>;
}
