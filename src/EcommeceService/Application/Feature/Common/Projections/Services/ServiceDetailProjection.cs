using Application.Feature.Common.Projections.Categories;
using Application.Feature.Common.Projections.Units;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceDetailProjection : ServiceProjection
    {
        public CategoryProjection Category { get; set; } = default!;
        public string? Description { get; set; }
        public long BranchId { get; set; } = default!;
        public List<UnitRelationProjection> UnitRelations { get; set; } = [];
    }
}
