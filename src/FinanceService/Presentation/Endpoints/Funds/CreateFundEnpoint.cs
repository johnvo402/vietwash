using Application.Features.Funds.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Funds
{
    public class CreateFundBehaviorEnpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateFundCommand>.WithActionResult<ApiResponse>
    {
        [HttpPost(Router.FundRoute.Funds)]
        [SwaggerOperation(Tags = [Router.FundRoute.Tags], Summary = "create fund")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            [FromBody] CreateFundCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
