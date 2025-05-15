using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Units;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Command.Update
{
    public class UpdateServiceMapping : Profile
    {
        public UpdateServiceMapping()
        {
            CreateMap<ServiceModel, Service>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => Ulid.Parse(src.CategoryId)))
                .ForMember(dest => dest.UnitRelations, opt => opt.Ignore());

            CreateMap<UnitRelationModel, UnitRelation>()
                .ForMember(dest => dest.BaseUnit, opt => opt.MapFrom(src => src.BaseUnit))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

            CreateMap<Service, UpdateServiceResponse>()
                .IncludeBase<Service, ServiceDetailProjection>();
        }
    }
}
