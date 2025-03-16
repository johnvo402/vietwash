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
    : EndpointBaseAsync.WithRequest<UpdateCategoryCommand>.WithActionResult<
        ApiResponse<Mediator.Unit>
    >
{
    [HttpPut(Router.CategoryRoute.GetUpdateDelete)]
    [SwaggerOperation(Tags = [Router.CategoryRoute.Tags], Summary = "Update category")]
    public override async Task<ActionResult<ApiResponse<Mediator.Unit>>> HandleAsync(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await sender.Send(request);
            return this.Created201();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }
}
