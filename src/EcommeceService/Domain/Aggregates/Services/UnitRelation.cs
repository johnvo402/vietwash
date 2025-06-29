using Domain.Aggregates.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Services.Enums;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Services
{
    public class UnitRelation : BaseEntity
    {
        public long ReferenceId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public bool BaseUnit { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public int Multiple { get; set; } = default!;
        public decimal ProcessingTime { get; set; } = default!;
        public ActivationStatus Status { get; set; } = default!;

        public Service Service { get; set; } = default!;

        //public Product Product { get; set; } = default!;
        public ICollection<OrderItem> OrderItems { get; set; } = [];

        public ICollection<ProductSupplying>? ProductSupplyings { get; set; }
        public ICollection<EquipmentSupplying>? EquipmentSupplyings { get; set; }

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
