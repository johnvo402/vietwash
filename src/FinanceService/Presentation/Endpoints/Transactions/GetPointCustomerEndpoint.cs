using Application.Common.Auth;
using Application.Features.Funds.Queries.Detail;
using Application.Features.Transactions.Queries.PointCustomer;
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
        : EndpointBaseAsync.WithoutRequest.WithActionResult<ApiResponse<PointCustomerResponse>>
    {
        [HttpGet(Router.TransactionRoute.GetCustomerPoint)]
        [SwaggerOperation(Tags = [Router.TransactionRoute.Tags], Summary = "Transaction")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse<PointCustomerResponse>>> HandleAsync(
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(new PointCustomerQuery(), cancellationToken);
            return result.ToActionResult();
        }
    }
}
