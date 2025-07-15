using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Orders.Queries.DetailByCode
{
    public record GetOrderDetailByCodeQuery : IRequest<Result<GetOrderDetailByCodeResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public string Code { get; set; } = default!;
    }
}
