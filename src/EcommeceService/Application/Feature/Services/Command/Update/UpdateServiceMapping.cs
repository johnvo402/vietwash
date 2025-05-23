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
            CreateMap<UpdateServiceModel, Service>()
				.ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
				.ForMember(dest => dest.UnitRelations, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
				.ForMember(dest => dest.OrderItems, opt => opt.Ignore())
				.ForMember(dest => dest.GroupServices, opt => opt.Ignore())
				.ForMember(dest => dest.ServicePriceTariffHistories, opt => opt.Ignore());

			CreateMap<UpdateUnitRelationModel, UnitRelation>()
				.ForMember(dest => dest.ReferenceId, opt => opt.Ignore())
				.ForMember(dest => dest.Service, opt => opt.Ignore())
				.ForMember(dest => dest.OrderItems, opt => opt.Ignore());

			CreateMap<Service, UpdateServiceResponse>()
                .IncludeBase<Service, ServiceDetailProjection>();
        }
    }
}
