using Domain.Aggregates.Orders;
using Domain.Aggregates.Warehouses;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Services
{
    public class UnitRelation : DefaultEntity
	{
        public long ReferenceId { get; set; } = default!;
		public long BranchId { get; set; } = default!;
		public string Name { get; set; } = default!;
        public bool BaseUnit { get; set; } = default!;
		public decimal Price { get; set; } = default!;
        public int Multiple { get; set; } = default!;
        public decimal ProcessingTime { get; set; } = default!;

		public Service Service { get; set; } = default!;
		public Product Product { get; set; } = default!;
		public ICollection<OrderItem> OrderItems { get; set; } = [];


	}
}
