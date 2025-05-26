using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Projections.Warehouses;
using AutoMapper;
using Domain.Aggregates.Warehouses;

namespace Application.Features.Warehouses.Commands.Create
{
    public class CreateWarehouseMapping : Profile
    {
        public CreateWarehouseMapping()
        {
            CreateMap<WarehouseModel, Warehouse>();
        }
    }
}
