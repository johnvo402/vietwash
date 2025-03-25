using Application.Feature.Common.Projections.Units;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Common.Mapping.Units
{
    public class UnitMapping : Profile
    {
        public UnitMapping()
        {
            CreateMap<UnitModel, Unit>();
            CreateMap<UnitRelationModel, UnitRelation>();
            CreateMap<UnitRelation, UnitRelationModel>();
        }
    }
}
