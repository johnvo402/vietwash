using Application.Common.Auth;
using Application.Feature.Tariffs.Commands.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Tariffs
{
    public class CreateTariffEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateTariffCommand>.WithActionResult<ApiResponse>
    {
        [HttpPost(Router.TariffRoute.Tariffs)]
        [SwaggerOperation(Tags = [Router.TariffRoute.Tags], Summary = "Create Tariff")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            [FromBody] CreateTariffCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var tariff = await sender.Send(request, cancellationToken);
            return tariff.ToCreatedResult();
        }
    }
}
