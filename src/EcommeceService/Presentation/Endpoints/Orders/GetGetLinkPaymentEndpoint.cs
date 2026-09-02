using Application.Common.Auth;
using Application.Feature.Orders.Queries.GetLinkPayment;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders
{
    public class GetGetLinkPaymentEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetLinkPaymentQuery>.WithActionResult<
            ApiResponse<CreatePaymentResult>
        >
    {
        [HttpPost(Router.OrderRoute.GetLinkPayment, Name = "CreateOrReuseOrderPaymentLink")]
        [SwaggerOperation(
            Tags = [Router.OrderRoute.Tags],
            Summary = "Create or reuse an order payment link"
        )]
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
        public override async Task<ActionResult<ApiResponse<CreatePaymentResult>>> HandleAsync(
            [FromRoute] GetLinkPaymentQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var response = await sender.Send(request, cancellationToken);
            return response.ToActionResult();
        }
    }
}
