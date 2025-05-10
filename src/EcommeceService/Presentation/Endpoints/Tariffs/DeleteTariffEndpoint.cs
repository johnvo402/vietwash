using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Auth;
using Application.Feature.Tariffs.Commands.Delete;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Contracts.Routers;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Tariffs
{
    public class DeleteTariffEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<string>.WithActionResult
    {
        [HttpDelete(Router.TariffRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.TariffRoute.Tags], Summary = "Delete Tariff")]
        //[AuthorizeBy(permissions: $"{ActionPermission.delete}:{ObjectPermission.tariff}")]
        public override async Task<ActionResult> HandleAsync(
            [FromRoute(Name = RouterBase.Id)] string tariffId,
            CancellationToken cancellationToken = default)
        {
            await sender.Send(new DeleteTariffCommand(Ulid.Parse(tariffId)), cancellationToken);
            return this.NoContent204();
        }

    }
}