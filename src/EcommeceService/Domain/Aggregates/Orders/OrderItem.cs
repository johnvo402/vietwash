using Domain.Aggregates.Services;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Orders
{
    public class OrderItem : BaseEntity
    {

        public Ulid OrderId { get; set; }
        public Ulid ServiceId { get; set; }
        public Ulid UnitRelationId { get; set; }

        public virtual Service Service { get; set; } = default!;
        public virtual Order Order { get; set; } = default!;
        public virtual UnitRelation UnitRelation { get; set; } = default!;
    }
}
