using Domain.Aggregates.Services;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Orders
{
    public class OrderItem : DefaultEntity
    {

        public long OrderId { get; set; }
        public long ServiceId { get; set; }
        public long UnitRelationId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public virtual Service Service { get; set; } = default!;
        public virtual Order Order { get; set; } = default!;
        public virtual UnitRelation UnitRelation { get; set; } = default!;
    }
}
