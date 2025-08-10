using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Enums;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceModel
    {
        public long BranchId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public ActivationStatus Status { get; set; } = default!;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public long CategoryId { get; set; } = default!;
        public List<UnitRelationModel> UnitRelations { get; set; } = [];
    }
}
