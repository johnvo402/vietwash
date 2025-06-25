using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Mediator;

namespace Application.Features.FundBehaviors.Queries
{
    public class ListFundBehaviorQuery
        : QueryParamRequest,
            IRequest<Result<IEnumerable<ListFundBehaviorResponse>>>;
}
