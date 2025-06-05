using Domain.Aggregates.Inventories;
using Domain.Aggregates.Suppliers.Enum;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Suppliers
{
    public class Supplier : AggregateRoot
	{
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Description { get; set; } = default!;
        public SupplierStatus Status { get; set; } = default!;
		public bool Disable { get; set; } = default!;
		public long BranchId { get; set; } = default!;
        public string? Image { get; set; }
        public ICollection<ProductSupplying> ProductSupplyings { get; set; } = [];

        //public ICollection<InventoryRequest> InventoryRequests { get; set; } = [];
        //public ICollection<InventoryInvoke> InventoryInvokes { get; set; } = [];
        public ICollection<EquipmentSupplying> EquipmentSupplyings { get; set; } = [];

		protected override bool TryApplyDomainEvent(INotification domainEvent)
		{
			throw new NotImplementedException();
		}
	}
}
