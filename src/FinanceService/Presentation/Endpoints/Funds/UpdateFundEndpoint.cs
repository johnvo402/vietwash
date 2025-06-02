using Application.Features.Funds.Command.Update;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Funds
{

    public class UpdateFundBehaviorEndpoint(ISender sender)
: EndpointBaseAsync.WithRequest<UpdateFundCommand>.WithActionResult<ApiResponse<Unit>>
    {
        [HttpPut(Router.FundRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.FundRoute.Tags], Summary = "Update fund")]
        public override async Task<ActionResult<ApiResponse<Unit>>> HandleAsync(
            UpdateFundCommand command,
            CancellationToken cancellationToken = default
        ) => this.Ok200(await sender.Send(command, cancellationToken));

    }
}

