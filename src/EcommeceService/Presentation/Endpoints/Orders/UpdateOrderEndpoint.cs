using Application.Common.Auth;
using Application.Feature.Orders.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders
{
    public class UpdateOrderEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<UpdateOrderCommand>.WithActionResult<ApiResponse>
    {
        [HttpPut(Router.OrderRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "Update order")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            UpdateOrderCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var order = await sender.Send(request, cancellationToken);
            return order.ToActionResult();
        }
    }
}
