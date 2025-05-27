using Application.Feature.Categories.Queries.List;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Categories;

public class ListCategoryEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListCategoryQuery>.WithActionResult<
        ApiResponse<IEnumerable<ListCategoryResponse>>
    >
{
    [HttpGet(Router.CategoryRoute.Categories)]
    [SwaggerOperation(Tags = [Router.CategoryRoute.Tags], Summary = "Category list")]
    public override async Task<
        ActionResult<ApiResponse<IEnumerable<ListCategoryResponse>>>
    > HandleAsync(ListCategoryQuery request, CancellationToken cancellationToken = default) =>
        this.Ok200(await sender.Send(request, cancellationToken));
}