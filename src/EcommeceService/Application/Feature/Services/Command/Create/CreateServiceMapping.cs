using Application.Feature.Common.Projections.Units;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Command.Create
{
	public class CreateServiceMapping : Profile
	{
		public CreateServiceMapping()
		{
			CreateMap<CreateServiceCommand, Service>()
				.ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
				.ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.BranchId))
				.ForMember(dest => dest.UnitRelations, opt => opt.MapFrom(src => src.UnitRelations))
				.ForMember(dest => dest.Category, opt => opt.Ignore())
				.ForMember(dest => dest.OrderItems, opt => opt.Ignore())
				.ForMember(dest => dest.GroupServices, opt => opt.Ignore())
				.ForMember(dest => dest.ServicePriceTariffHistories, opt => opt.Ignore());

			CreateMap<UnitRelationModel, UnitRelation>()
				.ForMember(dest => dest.ReferenceId, opt => opt.Ignore())
				.ForMember(dest => dest.Service, opt => opt.Ignore())
				.ForMember(dest => dest.OrderItems, opt => opt.Ignore());
			CreateMap<UnitRelation, UnitRelation>();

		}
	}
}
