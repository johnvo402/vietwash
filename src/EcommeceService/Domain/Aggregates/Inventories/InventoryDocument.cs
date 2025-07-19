using Ardalis.GuardClauses;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Inventories.Events;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Inventories
{
    public class InventoryDocument : AggregateRoot
    {
        public decimal Amount { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public decimal PaidAmount { get; set; }
        public long? BranchId { get; set; }
        public DateTimeOffset? TransactionAt { get; set; }
        public string Code { get; set; } = null!;
        public InventoryStatus Status { get; set; }
        public InventoryType Type { get; set; }
        public string? Note { get; set; }
        public string? CancelReason { get; set; }

        public ICollection<InventoryRelation> InventoryRelationships { get; set; } = [];
        public ICollection<EquipmentSupplying> EquipmentSupplyings { get; set; } = [];
        public ICollection<ProductSupplying> ProductSupplyings { get; set; } = [];

        public void UpdateStatus(InventoryStatus status, string? cancelReason = null)
        {
            if (status != Status)
                Status = status;
            if (status == InventoryStatus.Completed)
            {
                TransactionAt = DateTimeOffset.UtcNow;
                if (Type == InventoryType.Import)
                {
                    Emit(new InventoryDocumentCompletedEvent { InventoryDocument = this });
                }
            }
            if (
                (
                    (Type == InventoryType.Export && status == InventoryStatus.Completed)
                    || status == InventoryStatus.Canceled
                ) && EquipmentSupplyings.Any()
            )
            {
                Emit(new InventoryDocumentCanceledEvent { InventoryDocument = this });
            }
            CancelReason = cancelReason;
        }

        public InventoryDocument(
            string code,
            decimal amount,
            InventoryType type,
            long? branchId,
            string? note = null
        )
        {
            // Validate required fields
            Code = Guard.Against.NullOrWhiteSpace(code, nameof(code));
            Type = Guard.Against.Null(type, nameof(type));
            Status = InventoryStatus.Pending;

            // Validate amount and paid amount
            Amount = Guard.Against.NegativeOrZero(amount, nameof(amount));
            BranchId = branchId;
            Note = note;
        }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            switch (domainEvent)
            {
                case InventoryDocumentCompletedEvent:
                    return true;
                case InventoryDocumentCanceledEvent:
                    return true;
                // Các event khác nếu có
                default:
                    return false;
            }
        }
    }
}
