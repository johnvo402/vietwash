using System;
using System.Collections.Generic;
using Application.Features.Common.Projections.Warehouses;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Warehouses.Commands.Update
{
    public class UpdateWarehouseCommand : IRequest<UpdateWarehouseResponse>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long WarehouseId { get; set; }
        [FromBody]
        public UpdateWarehouse? Warehouse { get; set; }
        public class UpdateWarehouse : WarehouseModel { }
    }
}
