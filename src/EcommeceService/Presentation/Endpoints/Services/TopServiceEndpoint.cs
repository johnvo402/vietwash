using Application.Common.Auth;
using Application.Feature.Services.Queries.TopService;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Services
{
    public class TopServiceEndpoint(ISender sender)
        : EndpointBaseAsync.WithoutRequest.WithActionResult<
            ApiResponse<IEnumerable<TopServiceResponse>>
        >
    {
        [HttpGet(Router.ServiceRoute.TopService)]
        [SwaggerOperation(Tags = [Router.ServiceRoute.Tags], Summary = "Top Service list")]
        // [AuthorizeBy(roles: ROLE.CUSTOMER)]
        public override async Task<
            ActionResult<ApiResponse<IEnumerable<TopServiceResponse>>>
        > HandleAsync(CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(new TopServiceQuery(), cancellationToken);
            return result.ToActionResult();
        }
    }
}
