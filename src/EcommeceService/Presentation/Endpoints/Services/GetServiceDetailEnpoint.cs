using Application.Common.Auth;
using Application.Feature.Services.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
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
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
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
