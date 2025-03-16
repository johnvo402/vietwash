using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Auth;
using Application.Feature.Tariffs.Commands.Create;
using Application.Feature.Tariffs.Commands.Delete;
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
    public class CreateTariffEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<CreateTariffCommand>.WithActionResult<ApiResponse<Unit>>
    {
        [HttpPost(Router.TariffRoute.Tariffs)]
        [SwaggerOperation(Tags = [Router.TariffRoute.Tags], Summary = "Create Tariff")]
        //[AuthorizeBy(permissions: $"{ActionPermission.create}:{ObjectPermission.tariff}")]
        public override async Task<ActionResult<ApiResponse<Unit>>> HandleAsync(
            [FromForm] CreateTariffCommand request, CancellationToken cancellationToken = default)
        {
            var tariff = await sender.Send(request, cancellationToken);
            return this.Created201();
        }
    }
}
