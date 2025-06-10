using Domain.Aggregates.Inventories.Enums;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Inventories
{
    public class InventoryRequest : BaseEntity
    {
        public long? SupplierId { get; set; }
        public InventoryDocumentStatus Status { get; set; }
        public string Note { get; set; }
        public InventoryDocumentType Type { get; set; }
        public DateTimeOffset? RequestAt { get; set; }
        public long? BranchId { get; set; }
        public string CancelReason { get; set; }
        public long? FromWarehouseId { get; set; }
        public long? ToWarehouseId { get; set; }
    }
}
