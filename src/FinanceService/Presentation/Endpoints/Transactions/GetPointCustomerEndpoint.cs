using Application.Common.Auth;
using Application.Features.Funds.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Transactions
{
    public class GetPointCustomerEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetFundDetailQuery>.WithActionResult<
            ApiResponse<GetFundDetailResponse>
        >
    {
        [HttpGet(Router.TransactionRoute.GetCustomerPoint)]
        [SwaggerOperation(Tags = [Router.TransactionRoute.Tags], Summary = "Transaction")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse<GetFundDetailResponse>>> HandleAsync(
            GetFundDetailQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
