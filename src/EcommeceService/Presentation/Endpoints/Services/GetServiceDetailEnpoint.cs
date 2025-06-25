using Application.Common.Auth;
using Application.Feature.Services.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
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
        : EndpointBaseAsync.WithRequest<GetServiceDetailQuery>.WithActionResult<
            ApiResponse<GetServiceDetailResponse>
        >
    {
        [HttpGet(Routes.Router.ServiceRoute.GetDetail)]
        [SwaggerOperation(Tags = [Routes.Router.ServiceRoute.Tags], Summary = "Detail service")]
        //[AuthorizeBy(permissions: $"{ActionPermission.detail}:{ObjectPermission.service}")]
        public override async Task<ActionResult<ApiResponse<GetServiceDetailResponse>>> HandleAsync(
            GetServiceDetailQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
