using Application.Feature.Categories.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Categories;

public class CreateCategoryEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<CreateCategoryCommand>.WithActionResult<ApiResponse<Unit>>
{
    [HttpPost(Router.CategoryRoute.Categories)]
    [SwaggerOperation(Tags = [Router.CategoryRoute.Tags], Summary = "Create category")]
    public override async Task<ActionResult<ApiResponse<Unit>>> HandleAsync(
        CreateCategoryCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(request);
        return this.Created201();
    }
}
