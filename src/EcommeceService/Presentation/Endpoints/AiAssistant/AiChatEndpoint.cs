using Application.Common.Auth;
using Application.Feature.AiAssistant.Commands.Chat;
using Application.Feature.AiAssistant.Models;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.AiAssistant;

public sealed class AiChatEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<AiChatCommand>.WithActionResult<
        ApiResponse<AiChatResponse>
    >
{
    [HttpPost(Router.AiRoute.Chat)]
    [SwaggerOperation(Tags = [Router.AiRoute.Tags], Summary = "Ask the VietWash business assistant")]
    [AuthorizeBy(roles: "ADMIN, MANAGER")]
    public override async Task<ActionResult<ApiResponse<AiChatResponse>>> HandleAsync(
        [FromBody] AiChatCommand request,
        CancellationToken cancellationToken = default
    )
    {
        Result<AiChatResponse> result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}
