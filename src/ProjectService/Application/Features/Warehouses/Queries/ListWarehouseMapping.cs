using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Projections.Warehouses;
using AutoMapper;
using Domain.Aggregates.Warehouses;

namespace Application.Features.Warehouses.Queries
{
    public class ListWarehouseMapping : Profile
    {
        public ListWarehouseMapping()
        {
            CreateMap<Warehouse, WarehouseProjection>();
            CreateMap<Warehouse, ListWarehouseResponse>().IncludeBase<Warehouse, WarehouseProjection>();
        }
    }
}
