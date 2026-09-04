using Domain.Aggregates.Enums;
using Domain.Aggregates.Inventories.Enums;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Inventories
{
    public class InventoryRequest : AggregateRoot
    {
        public long? SupplierId { get; set; }
        public ActivationStatus Status { get; set; }
        public string Note { get; set; } = default!;
        public InventoryRequestType Type { get; set; }
        public DateTimeOffset? RequestAt { get; set; }
        public string? CancelReason { get; set; }
        public long? FromBranchId { get; set; }
        public long? ToBranchId { get; set; }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
