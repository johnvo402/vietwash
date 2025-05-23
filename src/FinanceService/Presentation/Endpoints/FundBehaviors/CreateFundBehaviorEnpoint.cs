using Application.Features.FundBehaviors.Command;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.FundBehaviors
{
    public class CreateFundBehaviorEnpoint(ISender sender) : EndpointBaseAsync.WithRequest<CreateFundBehaviorCommand>.WithActionResult<ApiResponse<Unit>>
    {


        [HttpPost(Router.FundBehaviorRoute.FundBehaviors)]
        [SwaggerOperation(Tags = [Router.FundBehaviorRoute.Tags], Summary = "create fundBehavior")]
        public override async Task<ActionResult<ApiResponse<Unit>>> HandleAsync([FromBody] CreateFundBehaviorCommand request, CancellationToken cancellationToken = default)
        {
            await sender.Send(request, cancellationToken);
            return this.Created201();
        }
    }

}
