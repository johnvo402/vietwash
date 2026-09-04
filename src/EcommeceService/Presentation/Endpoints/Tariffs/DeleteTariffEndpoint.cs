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
        : EndpointBaseAsync.WithRequest<DeleteTariffCommand>.WithActionResult<ApiResponse>
    {
        [HttpDelete(Router.TariffRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.TariffRoute.Tags], Summary = "Delete Tariff")]
        [AuthorizeBy(roles: "ADMIN")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
			DeleteTariffCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToNoContentResult();
        }
    }
}
