using Application.UseCases.AuditLogs.Queries;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.AuditLogs;

public class ListAuditLog(ISender sender)
    : EndpointBaseAsync.WithRequest<ListAuditlogQuery>.WithActionResult<ApiResponse<PaginationResponse<ListAuditlogResponse>>>
{
    [HttpGet(Router.AuditLogRoute.AuditLog)]
    [SwaggerOperation(Tags = [Router.AuditLogRoute.Tags], Summary = "List audit log")]
    public override async Task<ActionResult<ApiResponse<PaginationResponse<ListAuditlogResponse>>>> HandleAsync(
        [FromQuery] ListAuditlogQuery request,
        CancellationToken cancellationToken = default
    )
    {
        return this.Ok200(await sender.Send(request, cancellationToken));
    }
}
