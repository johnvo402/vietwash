using Application.Common.Auth;
using Application.Feature.Services.Queries.List;
using Application.Feature.Services.Queries.ServiceOrderReport;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Reports
{
	public class ServiceOrderReportEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<ServiceRevenueReportQuery>.WithActionResult<
		ApiResponse<List<ServiceRevenueReportResponse>>
	>
	{
		[HttpGet(Router.ReportRoute.ReportServiceOrder)]
		[SwaggerOperation(Tags = [Router.ReportRoute.Tags], Summary = "Report service order")]
		[AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.reportservice}")]
		public override async Task<ActionResult<ApiResponse<List<ServiceRevenueReportResponse>>>> HandleAsync(
			[FromQuery] ServiceRevenueReportQuery request, 
			CancellationToken cancellationToken = default
			) => this.Ok200(await sender.Send(request, cancellationToken));
	}
}
