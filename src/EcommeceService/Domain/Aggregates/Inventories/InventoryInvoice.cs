using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Inventories.Enums;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Inventories
{
    public class InventoryInvoice : BaseEntity
    {
        public long? SupplierId { get; set; }
        public Status Status { get; set; }
        public decimal Amount { get; set; }
        public ICollection<InventoryRelation> InventoryRelationships = [];
    }
}
