using Application.Common.Auth;
using Application.Features.Funds.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Funds
{


    public class GetFundDetailEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<GetFundDetailQuery>.WithActionResult<ApiResponse<GetFundDetailResponse>>
    {
        [HttpGet(Router.FundRoute.GetUpdateDelete, Name = Router.FundRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.FundRoute.Tags], Summary = "Detail Order")]
        //[AuthorizeBy(permissions: $"{ActionPermission.detail}:{ObjectPermission.order}")]
        public override async Task<ActionResult<ApiResponse<GetFundDetailResponse>>> HandleAsync(
            GetFundDetailQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var response = await sender.Send(new GetFundDetailQuery { FundId = request.FundId }, cancellationToken);
            return this.Ok200(response);
        }
    }
}
