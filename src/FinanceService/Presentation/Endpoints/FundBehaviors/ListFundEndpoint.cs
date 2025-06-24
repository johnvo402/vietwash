using Application.Features.FundBehaviors.Queries;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.FundBehaviors
{
    public class ListFundBehaviorEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListFundBehaviorQuery>.WithActionResult<
            ApiResponse<IEnumerable<ListFundBehaviorResponse>>
        >
    {
        [HttpGet(Router.FundBehaviorRoute.FundBehaviors)]
        [SwaggerOperation(Tags = [Router.FundBehaviorRoute.Tags], Summary = "list Fundbehavior")]
        public override async Task<
            ActionResult<ApiResponse<IEnumerable<ListFundBehaviorResponse>>>
        > HandleAsync(ListFundBehaviorQuery request, CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
