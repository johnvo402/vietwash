using Application.Common.Auth;
using Application.Feature.Services.Queries.Detail;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Contracts.Routers;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Services
{
    public class GetServiceDetailEnpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<long>.WithActionResult<
            ApiResponse<GetServiceDetailResponse>
        >
    {
        [HttpGet(Presentation.Routes.Router.ServiceRoute.GetDetail)]
        [SwaggerOperation(
            Tags = [Presentation.Routes.Router.ServiceRoute.Tags],
            Summary = "Detail service"
        )]
        //[AuthorizeBy(permissions: $"{ActionPermission.detail}:{ObjectPermission.service}")]
        public override async Task<ActionResult<ApiResponse<GetServiceDetailResponse>>> HandleAsync(
            [FromRoute(Name = RouterBase.Id)] long serviceId,
            CancellationToken cancellationToken = default
        ) => this.Ok200(await sender.Send(new GetServiceDetailQuery(serviceId), cancellationToken));
    }
}
