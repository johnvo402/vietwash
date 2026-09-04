using Application.Common.Auth;
using Application.Feature.Tariffs.Commands.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Tariffs
{
    public class UpdateTariffEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<UpdateTariffCommand>.WithActionResult<ApiResponse>
	{
		[HttpPut(Router.TariffRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.TariffRoute.Tags], Summary = "Update tariff")]
		[AuthorizeBy(roles: "ADMIN")]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			UpdateTariffCommand request,
			CancellationToken cancellationToken = default
		)
		{
			var result = await sender.Send(request);
			return result.ToActionResult();
		}
	}
}
