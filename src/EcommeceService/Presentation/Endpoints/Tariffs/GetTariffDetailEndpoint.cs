using Application.Common.Auth;
using Application.Feature.Tariffs.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Tariffs
{
	public class GetTariffDetailEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<GetTariffDetailQuery>.WithActionResult<
			ApiResponse<GetTariffDetailResponse>
		>
	{
		[HttpGet(Routes.Router.TariffRoute.GetDetail)]
		[SwaggerOperation(Tags = [Routes.Router.TariffRoute.Tags], Summary = "Detail tariff")]
		[AuthorizeBy]
		public override async Task<ActionResult<ApiResponse<GetTariffDetailResponse>>> HandleAsync(
			GetTariffDetailQuery request,
			CancellationToken cancellationToken = default
		)
		{
			var result = await sender.Send(request, cancellationToken);
			return result.ToActionResult();
		}
	}
}
