using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Inventories
{
    class InventoryDocument : AggregateRoot
    {
        public long? ToWarehouseId { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
        public short PaymentMethod { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public decimal PaidAmount { get; set; }
        public long? BranchId { get; set; }
        public long? FromWarehouseId { get; set; }
        public DateTimeOffset? TransactionAt { get; set; }
        public string Code { get; set; } = null!;
        public short Status { get; set; }
        public short Type { get; set; }
        public string? Note { get; set; }
        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
