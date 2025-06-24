using Application.Features.Common.Projections.Warehouses;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Warehouses.Commands.Update
{
    public class UpdateWarehouseCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long WarehouseId { get; set; }

        [FromBody]
        public WarehouseModel? Warehouse { get; set; }
    }
}
