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
        [HttpGet(Router.OrderRoute.GetLinkPayment, Name = Router.OrderRoute.GetLinkPayment)]
        [SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "Get link payment")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse<CreatePaymentResult>>> HandleAsync(
            GetLinkPaymentQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var response = await sender.Send(request, cancellationToken);
            return response.ToActionResult();
        }
    }
}
