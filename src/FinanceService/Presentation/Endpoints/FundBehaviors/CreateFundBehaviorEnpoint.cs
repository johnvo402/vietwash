using Application.Features.FundBehaviors.Command;
using Application.Common.Auth;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.FundBehaviors
{
    public class CreateFundBehaviorEnpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateFundBehaviorCommand>.WithActionResult<ApiResponse>
    {
        [HttpPost(Router.FundBehaviorRoute.FundBehaviors)]
        [SwaggerOperation(Tags = [Router.FundBehaviorRoute.Tags], Summary = "create fundBehavior")]
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            [FromBody] CreateFundBehaviorCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToCreatedResult();
        }
    }
}
