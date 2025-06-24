using Application.Feature.Categories.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Categories;

public class UpdateCategoryEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<UpdateCategoryCommand>.WithActionResult<ApiResponse>
{
    [HttpPut(Router.CategoryRoute.GetUpdateDelete)]
    [SwaggerOperation(Tags = [Router.CategoryRoute.Tags], Summary = "Update category")]
    public override async Task<ActionResult<ApiResponse>> HandleAsync(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request);
        return result.ToActionResult();
    }
}
