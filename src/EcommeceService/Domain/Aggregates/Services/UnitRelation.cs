using Domain.Aggregates.Orders;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Services
{
    public class UnitRelation : DefaultEntity
    {
        public long ServiceId { get; set; }
        public long UnitId { get; set; }
        public bool BaseUnit { get; set; }
        public decimal Price { get; set; }

        public Service Service { get; set; } = default!;
        public Unit Unit { get; set; } = default!;

        public ICollection<GroupService> GroupServices { get; set; } = [];
        public ICollection<OrderItem> OrderItems { get; set; } = [];


    }
}
