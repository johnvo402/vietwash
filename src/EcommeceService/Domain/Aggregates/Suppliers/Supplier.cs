using Domain.Aggregates.Inventories;
using Domain.Aggregates.Suppliers.Enum;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Suppliers
{
    public class Supplier : BaseEntity<long>
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Description { get; set; } = default!;
        public SupplierStatus Status { get; set; } = default!;
        public long BranchId { get; set; } = default!;
        public ICollection<ProductSupplying> ProductSupplyings { get; set; } = [];

        //public ICollection<InventoryRequest> InventoryRequests { get; set; } = [];
        //public ICollection<InventoryInvoke> InventoryInvokes { get; set; } = [];
        public ICollection<EquipmentSupplying> EquipmentSupplyings { get; set; } = [];
    }
}
