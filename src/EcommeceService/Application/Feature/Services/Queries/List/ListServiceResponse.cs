using Application.Feature.Common.Projections.Services;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Queries.List;

public class ListServiceResponse : ServiceProjection;


public class CategoryService
{
    public string? Name { get; set; }

    public virtual void MappingFrom(Category category)
    {
        Name = category.Name;
    }
}
