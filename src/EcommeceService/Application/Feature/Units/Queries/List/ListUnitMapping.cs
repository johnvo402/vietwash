using Application.Feature.Common.Projections.Units;
using AutoMapper;
using Domain.Aggregates.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Units.Queries.List
{
    public class ListUnitMapping : Profile
    {
        public ListUnitMapping()
        {
            CreateMap<Unit, UnitProjection>();
            CreateMap<Unit, ListUnitResponse>().IncludeBase<Unit, UnitProjection>();
        }
    }
}
