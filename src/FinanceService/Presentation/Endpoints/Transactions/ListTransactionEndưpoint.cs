using Application.Common.Auth;
using Application.Features.Transactions.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Transactions;

public class ListTransactionEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListTransactionQuery>.WithActionResult<
        ApiResponse<PaginationResponse<ListTransactionResponse>>
    >
{
    [HttpGet(Router.TransactionRoute.Transaction)]
    [SwaggerOperation(Tags = [Router.TransactionRoute.Tags], Summary = "list Transaction")]
    [AuthorizeBy]
    public override async Task<
        ActionResult<ApiResponse<PaginationResponse<ListTransactionResponse>>>
    > HandleAsync(
        [FromQuery] ListTransactionQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}
