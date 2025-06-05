using System;
using System.Collections.Generic;
using Application.Features.Common.Projections.Warehouses;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Warehouses.Commands.Update
{
    public class UpdateWarehouseCommand : IRequest<string>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long WarehouseId { get; set; }
        [FromBody]
        public WarehouseModel? Warehouse { get; set; }
    }
}
