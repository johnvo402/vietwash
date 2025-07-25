using Domain.Aggregates.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Products;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Services
{
    public class UnitRelation : BaseEntity
    {
        public long? ServiceId { get; set; }
        public long? BranchProductId { get; set; }
        public string Name { get; set; } = default!;
        public bool BaseUnit { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public int Multiple { get; set; } = default!;
        public decimal ProcessingTime { get; set; } = default!;
        public ActivationStatus Status { get; set; } = default!;
        public long? UnitId { get; set; }
        public Unit? Unit { get; set; }
        public Service? Service { get; set; } = default!;
        public BranchProduct? BranchProduct { get; set; } = default!;
        public ICollection<OrderItem> OrderItems { get; set; } = [];
        public ICollection<ProductSupplying>? ProductSupplyings { get; set; }

        public void Update(
            string? name = null,
            bool? baseUnit = null,
            decimal? price = null,
            int? multiple = null,
            decimal? processingTime = null,
            ActivationStatus? status = null
        )
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;
            if (baseUnit.HasValue)
                BaseUnit = baseUnit.Value;
            if (price.HasValue)
                Price = price.Value;
            if (multiple.HasValue)
                Multiple = multiple.Value;
            if (processingTime.HasValue)
                ProcessingTime = processingTime.Value;
            if (status.HasValue)
                Status = status.Value;
        }
    }
}
