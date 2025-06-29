using Domain.Aggregates.Enums;
using Domain.Aggregates.Inventories.Enums;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Inventories
{
    public class InventoryRequest : BaseEntity
    {
        public long? SupplierId { get; set; }
        public ActivationStatus Status { get; set; }
        public string Note { get; set; }
        public Enums.Type Type { get; set; }
        public DateTimeOffset? RequestAt { get; set; }
        public long? BranchId { get; set; }
        public string CancelReason { get; set; }
        public long? FromWarehouseId { get; set; }
        public long? ToWarehouseId { get; set; }
    }
}
