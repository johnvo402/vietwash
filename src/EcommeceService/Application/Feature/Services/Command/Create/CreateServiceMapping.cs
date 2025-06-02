using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Units;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Command.Create
{
	public class CreateServiceMapping : Profile
	{
		public CreateServiceMapping()
		{
			CreateMap<CreateServiceCommand, Service>().IncludeBase<ServiceModel, Service>();

			CreateMap<UnitRelationModel, UnitRelation>();
			CreateMap<UnitRelation, UnitRelation>();

		}
	}
}
