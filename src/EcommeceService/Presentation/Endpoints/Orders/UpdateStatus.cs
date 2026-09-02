using Application.Common.Auth;
using Application.Feature.Orders.Command.UpdateStatus;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders
{
    public class UpdateStatus(ISender sender)
        : EndpointBaseAsync.WithRequest<UpdateStatusCommand>.WithActionResult<ApiResponse>
    {
        [HttpPut(Router.OrderRoute.UpdateStatus)]
        [SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "Update Status Order")]
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            UpdateStatusCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var response = await sender.Send(request, cancellationToken);
            return response.ToActionResult();
        }
    }
}
