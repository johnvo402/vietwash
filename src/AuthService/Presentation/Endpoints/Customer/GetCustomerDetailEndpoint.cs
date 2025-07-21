using Application.Common.Auth;
using Application.Features.Accounts.Queries.Detail;
using Application.Features.Customers.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Customer
{
	public class GetCustomerDetailEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<long>.WithActionResult<ApiResponse<GetCustomerDetailResponse>>
	{
		[HttpGet(Router.CustomerRoute.GetUpdateDelete, Name = Router.CustomerRoute.GetRouteName)]
		[SwaggerOperation(Tags = [Router.CustomerRoute.Tags], Summary = "Detail Customer")]
		[AuthorizeBy]
		public override async Task<ActionResult<ApiResponse<GetCustomerDetailResponse>>> HandleAsync(
			[FromRoute(Name = RouterBase.Id)] long id,
			CancellationToken cancellationToken = default
		)
		{
			var result = await sender.Send(new GetCustomerDetailQuery(id), cancellationToken);
			return result.ToActionResult();
		}
	}
}
