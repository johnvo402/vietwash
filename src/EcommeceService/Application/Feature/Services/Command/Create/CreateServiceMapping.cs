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
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => Ulid.Parse(src.CategoryId)))
                .ForMember(dest => dest.UnitRelations, opt => opt.MapFrom(
                    //src.UnitRelations.Select(unit => new UnitRelation
                    //{
                    //    UnitId = Ulid.Parse(unit.UnitId),
                    //    BaseUnit = unit.BaseUnit,
                    //    Price = unit.Price
                    //}).ToList()
                    src => src.UnitRelations
                ));

            CreateMap<UnitRelationModel, UnitRelation>()
                .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => Ulid.Parse(src.UnitId)));

        }
    }
}
