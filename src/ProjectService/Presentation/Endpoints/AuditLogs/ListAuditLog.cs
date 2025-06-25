using Application.UseCases.AuditLogs.Queries;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.AuditLogs;

public class ListAuditLog(ISender sender)
    : EndpointBaseAsync.WithRequest<ListAuditlogQuery>.WithActionResult<
        ApiResponse<PaginationResponse<ListAuditlogResponse>>
    >
{
    [HttpGet(Router.AuditLogRoute.AuditLog)]
    [SwaggerOperation(Tags = [Router.AuditLogRoute.Tags], Summary = "List audit log")]
    public override async Task<
        ActionResult<ApiResponse<PaginationResponse<ListAuditlogResponse>>>
    > HandleAsync(
        [FromQuery] ListAuditlogQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}
