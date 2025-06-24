using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Auth;
using Application.Feature.Tariffs.Commands.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Tariffs
{
    public class UpdateTariffEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<UpdateTariffCommand>.WithActionResult<
            ApiResponse<UpdateTariffResponse>
        >
    {
        [HttpPut(Router.TariffRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.TariffRoute.Tags], Summary = "Update Tariff")]
        //[AuthorizeBy(permissions: $"{ActionPermission.update}:{ObjectPermission.tariff}")]
        public override async Task<ActionResult<ApiResponse<UpdateTariffResponse>>> HandleAsync(
            [FromBody] UpdateTariffCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var response = await sender.Send(request, cancellationToken);
            return response.ToActionResult();
        }
    }
}
