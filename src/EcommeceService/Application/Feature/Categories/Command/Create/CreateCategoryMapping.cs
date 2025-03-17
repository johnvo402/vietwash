using Application.Feature.Common.Projections.Services;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Categories.Command.Create;

public class CreateCategoryMapping : Profile
{
    public CreateCategoryMapping()
    {
        CreateMap<CreateCategoryCommand, Category>().IncludeBase<CategoryModel, Category>();
    }
}
