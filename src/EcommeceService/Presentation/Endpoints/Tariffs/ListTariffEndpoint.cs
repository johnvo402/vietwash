using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Auth;
using Application.Feature.Tariffs.Queries;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Infrastructure.Constants;
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
        //[AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.tariff}")]
        public async override Task<
            ActionResult<ApiResponse<PaginationResponse<ListTariffResponse>>>
        > HandleAsync(ListTariffQuery request, CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
