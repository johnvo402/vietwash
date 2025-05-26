using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Projections.Warehouses;
using AutoMapper;
using Domain.Aggregates.Warehouses;
using static Application.Features.Warehouses.Commands.Update.UpdateWarehouseCommand;

namespace Application.Features.Warehouses.Commands.Update
{
    public class UpdateWarehouseMapping : Profile
    {
        public UpdateWarehouseMapping()
        {
            CreateMap<UpdateWarehouse, Warehouse>();
            CreateMap<Warehouse, UpdateWarehouseResponse>().IncludeBase<Warehouse, WarehouseDetailProjection>();
        }
    }
}
