using Application.Common.Auth;
using Application.Feature.Tariffs.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Tariffs
{
	public class ListTariffEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListTariffQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListTariffResponse>>
        >
    {
        [HttpGet(Router.TariffRoute.Tariffs)]
        [SwaggerOperation(Tags = [Router.TariffRoute.Tags], Summary = "List Tariff")]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListTariffResponse>>>
        > HandleAsync(
            [FromQuery] ListTariffQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
