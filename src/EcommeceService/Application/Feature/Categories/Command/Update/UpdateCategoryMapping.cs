using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Categories.Command.Update;

public class UpdateCategoryMapping : Profile
{
    public UpdateCategoryMapping()
    {
        CreateMap<UpdateCategoryCommand, Category>();
    }
}
