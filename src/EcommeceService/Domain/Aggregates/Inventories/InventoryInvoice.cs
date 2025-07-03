using Domain.Aggregates.Enums;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Inventories
{
    public class InventoryInvoice : AggregateRoot
    {
        public long? SupplierId { get; set; }
        public ActivationStatus Status { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset TransactionAt { get; set; }
        public ICollection<InventoryRelation> InventoryRelationships { get; set; } = [];

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
