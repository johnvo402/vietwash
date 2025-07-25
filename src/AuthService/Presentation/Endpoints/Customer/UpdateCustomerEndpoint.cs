using Application.Common.Auth;
using Application.Features.Customers.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Customer
{
	public class UpdateCustomerEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<UpdateCustomerCommand>.WithActionResult<
		ApiResponse
	>
	{
		[HttpPut(Router.CustomerRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.CustomerRoute.Tags], Summary = "Update Customer")]
		[AuthorizeBy]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			UpdateCustomerCommand command,
			CancellationToken cancellationToken = default
		) => (await sender.Send(command, cancellationToken)).ToActionResult();
	}
}
