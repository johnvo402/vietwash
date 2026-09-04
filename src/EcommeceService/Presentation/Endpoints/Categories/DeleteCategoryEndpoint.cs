using Application.Feature.Categories.Command.Delete;
using Application.Common.Auth;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Categories;

public class DeleteCategoryEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<string>.WithActionResult<ApiResponse>
{
    [HttpDelete(Router.CategoryRoute.GetUpdateDelete)]
    [SwaggerOperation(Tags = [Router.CategoryRoute.Tags], Summary = "Delete category")]
    [AuthorizeBy(roles: "ADMIN, MANAGER")]
    public override async Task<ActionResult<ApiResponse>> HandleAsync(
        [FromRoute(Name = RouterBase.Id)] string categoryId,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteCategoryCommand(categoryId), cancellationToken);
        return NoContent();
    }
}
