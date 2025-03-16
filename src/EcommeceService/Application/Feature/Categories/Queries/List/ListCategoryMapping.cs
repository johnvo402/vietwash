using Application.Feature.Common.Projections.Categories;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Categories.Queries.List;

public class ListCategoryMapping : Profile
{
    public ListCategoryMapping()
    {
        CreateMap<Category, CategoryProjection>();
        CreateMap<Category, ListCategoryResponse>().IncludeBase<Category, CategoryProjection>();
    }
}
