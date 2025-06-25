using Application.Feature.Orders.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders
{
    public class GetOrderDetailEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetOrderDetailQuery>.WithActionResult<
            ApiResponse<GetOrderDetailResponse>
        >
    {
        [HttpGet(Router.OrderRoute.GetUpdateDelete, Name = Router.OrderRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "Detail Order")]
        // [AuthorizeBy(permissions: $"{ActionPermission.detail}:{ObjectPermission.order}")]
        public override async Task<ActionResult<ApiResponse<GetOrderDetailResponse>>> HandleAsync(
            GetOrderDetailQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var response = await sender.Send(request, cancellationToken);
            return response.ToActionResult();
        }
    }
}
