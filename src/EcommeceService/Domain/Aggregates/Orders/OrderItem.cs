using Domain.Aggregates.Services;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Orders
{
    public class OrderItem : DefaultEntity
    {
        public long OrderId { get; set; }
        public long ServiceId { get; set; }
        public long UnitRelationId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? UnitRelationName { get; set; }
        public decimal ProcessingTime { get; set; }
        public string? ServiceName { get; set; }
        public decimal UnitPrice { get; set; }
        public virtual Service Service { get; set; } = default!;
        public virtual Order Order { get; set; } = default!;
        public virtual UnitRelation UnitRelation { get; set; } = default!;
    }
}
