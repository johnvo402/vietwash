using Application.Common.Auth;
using Application.Feature.Units.Command.Create;
using Application.Feature.Units.Command.Update;
using Ardalis.ApiEndpoints;
using AutoMapper;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Units
{
	public class UpdateUnitEndpoint(ISender sender, IMapper mapper)
	: EndpointBaseAsync.WithRequest<UpdateUnitCommand>.WithActionResult<ApiResponse<UpdateUnitResponse>>
	{
		[HttpPut(Router.UnitRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.UnitRoute.Tags], Summary = "Update Unit")]
		//[AuthorizeBy(permissions: $"{ActionPermission.update}:{ObjectPermission.unit}")]
		public override async Task<ActionResult<ApiResponse<UpdateUnitResponse>>> HandleAsync(
			UpdateUnitCommand request, 
			CancellationToken cancellationToken = default)
		{
			var response = await sender.Send(request, cancellationToken);
			return this.Ok200(response);
		}
	}
}
