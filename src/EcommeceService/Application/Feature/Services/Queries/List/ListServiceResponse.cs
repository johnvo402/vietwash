using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Queries.List;

public class ListServiceResponse : ServiceProjection
{
    public CategoryService? Category { get; set; }
    public ICollection<UnitRelationProjection> UnitRelations { get; set; } = [];
}

public class CategoryService
{
    public string? Name { get; set; }
}
