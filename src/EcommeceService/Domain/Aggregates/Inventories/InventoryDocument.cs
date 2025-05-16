using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Inventories.Enums;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Inventories
{
    public class InventoryDocument : AggregateRoot
    {
        public long? ToWarehouseId { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public decimal PaidAmount { get; set; }
        public long? BranchId { get; set; }
        public long? FromWarehouseId { get; set; }
        public DateTimeOffset? TransactionAt { get; set; }
        public string Code { get; set; } = null!;
        public Status Status { get; set; }
        public Enums.Type Type { get; set; }
        public string? Note { get; set; }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }

        public ICollection<InventoryRelation> InventoryRelationships = [];
        public ICollection<EquipmentSupplying> EquipmentSupplyings = [];
    }
}
