using Application.Common.Auth;
using Application.Features.Media;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Medias
{
    public class UploadMediaEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<UploadMediaCommand>.WithActionResult<
            ApiResponse<UploadMediaResponse>
        >
    {
        [HttpPost(Router.MediaRoute.Media)]
        [SwaggerOperation(Tags = [Router.MediaRoute.Tags], Summary = "Upload Media")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse<UploadMediaResponse>>> HandleAsync(
            [FromForm] UploadMediaCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var media = await sender.Send(request, cancellationToken);
            return media.ToActionResult();
        }
    }
}
