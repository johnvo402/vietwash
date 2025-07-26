using Application.Common.Auth;
using Application.Feature.Tariffs.Queries.ListTariffByBranch;
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
    public class ListTariffByBranchEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListTariffByBranchQuery>.WithActionResult<
            ApiResponse<IList<ListTariffByBranchResponse>>
        >
    {
        [HttpGet(Router.TariffRoute.TariffByBranch)]
        [SwaggerOperation(Tags = [Router.TariffRoute.Tags], Summary = "List Tariff")]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<IList<ListTariffByBranchResponse>>>
        > HandleAsync(
            [FromQuery] ListTariffByBranchQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
