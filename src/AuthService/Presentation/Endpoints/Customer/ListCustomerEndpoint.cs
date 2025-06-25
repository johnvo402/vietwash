using Application.Common.Auth;
using Application.Features.Customers.Queries.ListCustomer;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Customer
{
    public class ListCustomerEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListCustomerQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListCustomerResponse>>
        >
    {
        [HttpGet(Router.Customer.GetList)]
        [SwaggerOperation(Tags = [Router.Customer.Tags], Summary = "list Customer")]
        //[AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListCustomerResponse>>>
        > HandleAsync(
            [FromQuery] ListCustomerQuery request,
            CancellationToken cancellationToken = default
        ) => (await sender.Send(request, cancellationToken)).ToActionResult();
    }
}
