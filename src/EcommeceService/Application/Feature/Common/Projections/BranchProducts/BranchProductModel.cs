using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Enums;

namespace Application.Feature.Common.Projections.BranchProducts
{
    public class BranchProductModel
    {
        public long BranchId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Sku { get; set; } = default!;
        public decimal CapitalPrice { get; set; }
        public string? Image { get; set; }
        public long CategoryId { get; set; }

        public ActivationStatus Status { get; set; } = ActivationStatus.Active;
        public List<UnitRelationModel> UnitRelations { get; set; } = [];
    }
}
