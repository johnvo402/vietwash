using Application.Feature.Common.Projections.Units;
using Application.Feature.Units.Command.Update;
using AutoMapper;
using Domain.Aggregates.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Mapping.Units
{
    public class UnitMapping : Profile
    {
        public UnitMapping()
        {
            CreateMap<UnitModel, Unit>();
            CreateMap<UnitRelationModel, UnitRelation>();


        }
    }
}
