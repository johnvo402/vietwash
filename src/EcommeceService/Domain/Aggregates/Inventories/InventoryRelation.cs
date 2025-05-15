using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Inventories
{
    public class InventoryRelation : BaseEntity
    {
        public long? InventoryDocumentId { get; set; }
        public long? InventoryInvoiceId { get; set; }
        public decimal Amount { get; set; }
        public InventoryDocument InventoryDocument { get; set; } = default!;
        public InventoryInvoice InventoryInvoice { get; set; } = default!;
    }
}
