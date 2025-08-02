using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Orders.Queries.TotalOrderByStaff
{
    public class TotalOrderByStaffQuery : IRequest<Result<TotalOrderByStaffResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long StaffId { get; set; }
    };
}
