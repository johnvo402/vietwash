using Application.Features.EInvoices.Queries.GetByCode;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.EInvoices
{
    public class GetEInvoiceByCodeEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetEInvoiceByCodeQuery>.WithActionResult<
            ApiResponse<GetEInvoiceByCodeResponse>
        >
    {
        [HttpGet(Router.EInvoiceRoute.GetByCode)]
        [SwaggerOperation(Tags = [Router.EInvoiceRoute.Tags], Summary = "get EInvoiceRoute")]
        public override async Task<
            ActionResult<ApiResponse<GetEInvoiceByCodeResponse>>
        > HandleAsync(GetEInvoiceByCodeQuery request, CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
