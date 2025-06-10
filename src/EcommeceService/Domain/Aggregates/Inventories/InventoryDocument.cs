using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Services;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Inventories
{
    public class InventoryDocument : AggregateRoot
    {
        public long? ToWarehouseId { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
        public InventoryPaymentMethod PaymentMethod { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public decimal PaidAmount { get; set; }
        public long? BranchId { get; set; }
        public long? FromWarehouseId { get; set; }
        public DateTimeOffset? TransactionAt { get; set; }
        public string Code { get; set; } = null!;
        public InventoryDocumentStatus Status { get; set; }
        public InventoryDocumentType Type { get; set; }
        public string? Note { get; set; }

        public ICollection<InventoryRelation> InventoryRelationships { get; set; } = [];
        public ICollection<EquipmentSupplying> EquipmentSupplyings { get; set; } = [];
        public ICollection<ProductSupplying> ProductSupplyings { get; set; } = [];


        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
