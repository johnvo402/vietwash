using Application.Common.Auth;
using Application.Features.Funds.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Funds
{
    public class UpdateFundBehaviorEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<UpdateFundCommand>.WithActionResult<ApiResponse>
    {
        [HttpPut(Router.FundRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.FundRoute.Tags], Summary = "Update fund")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            UpdateFundCommand command,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(command, cancellationToken);
            return result.ToActionResult();
        }
    }
}
