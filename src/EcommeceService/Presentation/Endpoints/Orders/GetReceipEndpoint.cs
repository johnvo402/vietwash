using Application.Common.Auth;
using Application.Feature.Orders.Queries.GetReceipt;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders
{
    public class GetReceiptEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetReceiptQuery>.WithActionResult<
            ApiResponse<GetReceiptResponse>
        >
    {
        [HttpGet(Router.OrderRoute.GetReceipt, Name = Router.OrderRoute.GetReceipt)]
        [SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "Get Receipt Order")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse<GetReceiptResponse>>> HandleAsync(
            GetReceiptQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var response = await sender.Send(request, cancellationToken);
            return response.ToActionResult();
        }
    }
}
