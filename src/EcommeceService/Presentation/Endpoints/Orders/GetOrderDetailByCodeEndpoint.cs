using Application.Common.Auth;
using Application.Feature.Orders.Queries.DetailByCode;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders
{
    public class GetOrderDetailByCodeEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetOrderDetailByCodeQuery>.WithActionResult<
            ApiResponse<GetOrderDetailByCodeResponse>
        >
    {
        [HttpPost(Router.OrderRoute.GetByCode, Name = Router.OrderRoute.GetByCode)]
        [SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "DetailByCode Order")]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<GetOrderDetailByCodeResponse>>
        > HandleAsync(
            [FromBody] GetOrderDetailByCodeQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var response = await sender.Send(request, cancellationToken);
            return response.ToActionResult();
        }
    }
}
