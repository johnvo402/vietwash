using Application.Feature.Categories.Command.Delete;
using Ardalis.ApiEndpoints;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Categories;

public class DeleteCategoryEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<string>.WithActionResult
{
    [HttpDelete(Router.CategoryRoute.GetUpdateDelete)]
    [SwaggerOperation(Tags = [Router.CategoryRoute.Tags], Summary = "Delete category")]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute(Name = RouterBase.Id)] string categoryId,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteCategoryCommand(categoryId), cancellationToken);
        return NoContent();
    }
}
