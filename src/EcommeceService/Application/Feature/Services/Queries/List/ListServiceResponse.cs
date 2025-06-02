using Application.Feature.Common.Projections.Services;

namespace Application.Feature.Services.Queries.List;

public class ListServiceResponse : ServiceProjection
{
    public CategoryService? Category { get; set; }
    public List<UnitRelationService> UnitRelations { get; set; } = [];
}

public class CategoryService
{
    public string? Name { get; set; }
}

public class UnitRelationService
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public bool BaseUnit { get; set; }
    public decimal Price { get; set; }

}