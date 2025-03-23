using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Units;
using Application.Feature.Services.Queries.List;
using AutoMapper;
using Domain.Aggregates.Services;
using Domain.Aggregates.Users;

namespace Application.Feature.Services.Queries.Detail
{
    public class GetServiceDetailMapping : Profile
    {
        public GetServiceDetailMapping()
        {

            CreateMap<Service, GetServiceDetailResponse>().IncludeBase<Service, ServiceProjection>();

            CreateMap<UnitRelation, UnitRelationProjection>()
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit));

            CreateMap<Unit, UnitProjection>();

            CreateMap<User, UserDTO>();
        }
    }
}
