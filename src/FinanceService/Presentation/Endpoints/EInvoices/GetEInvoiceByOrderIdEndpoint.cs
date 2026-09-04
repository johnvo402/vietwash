using Application.Common.Auth;
using Application.Features.EInvoices.Queries.GetByOrderId;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.EInvoices
{
    public class GetEInvoiceByOrderIdEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetEInvoiceByOrderIdQuery>.WithActionResult<
            ApiResponse<GetEInvoiceByOrderIdResponse>
        >
    {
        [HttpGet(Router.EInvoiceRoute.GetByOrderId)]
        [SwaggerOperation(Tags = [Router.EInvoiceRoute.Tags], Summary = "create EInvoiceRoute")]
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
        public override async Task<
            ActionResult<ApiResponse<GetEInvoiceByOrderIdResponse>>
        > HandleAsync(
            GetEInvoiceByOrderIdQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
