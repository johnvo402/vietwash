using Application.Common.Auth;
using Application.Feature.Feedbacks.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Feedbacks
{
    public class CreateFeedbackEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateFeedbackCommand>.WithActionResult<ApiResponse>
    {
        [HttpPost(Router.ServiceRoute.CreateFeedback)]
        [SwaggerOperation(Tags = [Router.ServiceRoute.Tags], Summary = "Create a new feedback")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            CreateFeedbackCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var tariff = await sender.Send(request, cancellationToken);
            return tariff.ToCreatedResult();
        }
    }
}
