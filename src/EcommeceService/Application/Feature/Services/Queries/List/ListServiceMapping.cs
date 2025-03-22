using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Units;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Queries.List;

public class ListServiceMapping : Profile
{
    public ListServiceMapping()
    {
        CreateMap<Service, ServiceProjection>()
            .ForMember(dest => dest.UnitRelations, opt => opt.MapFrom(src => src.UnitRelations));

        CreateMap<Service, ListServiceResponse>().IncludeBase<Service, ServiceProjection>();

        CreateMap<UnitRelation, UnitRelationProjection>()
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit));

        CreateMap<Unit, UnitProjection>();
    }
}
