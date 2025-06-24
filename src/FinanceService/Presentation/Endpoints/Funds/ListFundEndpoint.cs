using Application.Common.Auth;
using Application.Features.Funds.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Funds
{
    public class ListFundEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListFundQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListFundResponse>>
        >
    {
        [HttpGet(Router.FundRoute.Funds)]
        [SwaggerOperation(Tags = [Router.FundRoute.Tags], Summary = "list Fund")]
        //[AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.fund}")]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListFundResponse>>>
        > HandleAsync(
            [FromQuery] ListFundQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
