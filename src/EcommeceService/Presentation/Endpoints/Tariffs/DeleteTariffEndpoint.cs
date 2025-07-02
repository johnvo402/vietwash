using Application.Common.Auth;
using Application.Feature.Tariffs.Commands.Delete;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Tariffs
{
    public class DeleteTariffEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<long>.WithActionResult<ApiResponse>
    {
        [HttpDelete(Router.TariffRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.TariffRoute.Tags], Summary = "Delete Tariff")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            [FromRoute(Name = RouterBase.Id)] long tariffId,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(new DeleteTariffCommand(tariffId), cancellationToken);
            return result.ToNoContentResult();
        }
    }
}
