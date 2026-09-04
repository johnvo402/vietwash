using Application.Feature.Categories.Queries.List;
using Application.Common.Auth;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Categories;

public class ListCategoryEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListCategoryQuery>.WithActionResult<
        ApiResponse<PaginationResponse<ListCategoryResponse>>
    >
{
    [HttpGet(Router.CategoryRoute.Categories)]
    [SwaggerOperation(Tags = [Router.CategoryRoute.Tags], Summary = "Category list")]
    [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
    public override async Task<
        ActionResult<ApiResponse<PaginationResponse<ListCategoryResponse>>>
    > HandleAsync(
        [FromQuery] ListCategoryQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}
