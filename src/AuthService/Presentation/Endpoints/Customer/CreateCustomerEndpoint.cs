using Application.Common.Auth;
using Application.Features.Customers.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Customer
{
    public class CreateCustomerEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateCustomerCommand>.WithActionResult<
            ApiResponse<CreateCustomerResponse>
        >
    {
        [HttpPost(Router.CustomerRoute.Customers)]
        [SwaggerOperation(Tags = [Router.CustomerRoute.Tags], Summary = "Create customer")]
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
        public override async Task<ActionResult<ApiResponse<CreateCustomerResponse>>> HandleAsync(
            CreateCustomerCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToCreatedResult();
        }
    }
}
