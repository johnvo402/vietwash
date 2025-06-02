using Application.Features.FundBehaviors.Queries;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
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
        > HandleAsync(ListFundBehaviorQuery request, CancellationToken cancellationToken = default) =>
            this.Ok200(await sender.Send(request, cancellationToken));
    }
}
