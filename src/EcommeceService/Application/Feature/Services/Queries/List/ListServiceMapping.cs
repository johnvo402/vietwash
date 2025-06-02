using Application.Feature.Common.Projections.Categories;
using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Units;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Queries.List;

public class ListServiceMapping : Profile
{
    public ListServiceMapping()
    {

        CreateMap<Service, ListServiceResponse>().IncludeBase<Service, ServiceProjection>();

        CreateMap<UnitRelation, UnitRelationService>();

        CreateMap<Category, CategoryService>();
    }
}
