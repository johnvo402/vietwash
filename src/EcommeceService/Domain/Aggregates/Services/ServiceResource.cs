using Domain.Aggregates.Products;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Services
{
    public class ServiceResource : DefaultEntity<long>
    {
        public long UnitProductId { get; set; }

        public UnitRelation UnitProduct { get; set; } = null!;
        public long UnitRelationId { get; set; }

        public UnitRelation UnitRelation { get; set; } = null!;

        public long ProductId { get; set; }

        public BranchProduct BranchProduct { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}
