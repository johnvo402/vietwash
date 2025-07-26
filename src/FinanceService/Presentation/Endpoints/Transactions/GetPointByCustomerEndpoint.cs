using Application.Common.Auth;
using Application.Features.Funds.Queries.Detail;
using Application.Features.Transactions.Queries.GetPointCustomer;
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
    public class GetPointByCustomerEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetPointCustomerQuery>.WithActionResult<
            ApiResponse<GetPointCustomerResponse>
        >
    {
        [HttpGet(Router.TransactionRoute.GetPointByCustomerId)]
        [SwaggerOperation(Tags = [Router.TransactionRoute.Tags], Summary = "Transaction")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse<GetPointCustomerResponse>>> HandleAsync(
            GetPointCustomerQuery query,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToActionResult();
        }
    }
}
