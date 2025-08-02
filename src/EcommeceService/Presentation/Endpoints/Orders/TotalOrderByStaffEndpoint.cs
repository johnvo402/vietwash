using Application.Common.Auth;
using Application.Feature.Orders.Queries.TotalOrderByStaff;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders
{
    public class TotalOrderByStaffEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<TotalOrderByStaffQuery>.WithActionResult<
            ApiResponse<TotalOrderByStaffResponse>
        >
    {
        [HttpGet(Router.OrderRoute.GetByStaff)]
        [SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "get total Order")]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<TotalOrderByStaffResponse>>
        > HandleAsync(TotalOrderByStaffQuery request, CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
