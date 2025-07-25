using Application.Common.Auth;
using Application.Feature.Services.Queries.GetByTariff;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Services
{
    public class GetByTariffEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetByTariffQuery>.WithActionResult<
            ApiResponse<PaginationResponse<GetByTariffResponse>>
        >
    {
        [HttpGet(Routes.Router.ServiceRoute.ServicesByTariff)]
        [SwaggerOperation(Tags = [Routes.Router.ServiceRoute.Tags], Summary = "tariff service")]
        // [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<GetByTariffResponse>>>
        > HandleAsync(
            [FromQuery] GetByTariffQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
