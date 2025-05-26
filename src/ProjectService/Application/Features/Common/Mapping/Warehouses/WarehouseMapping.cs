using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Projections.Warehouses;
using AutoMapper;
using Domain.Aggregates.Warehouses;

namespace Application.Features.Common.Mapping.Warehouses
{
    public class WarehouseMapping : Profile
    {
        public WarehouseMapping()
        {
            CreateMap<Warehouse, WarehouseProjection>();
            CreateMap<Warehouse, WarehouseDetailProjection>();
        }
    }
}
