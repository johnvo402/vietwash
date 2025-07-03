using Shared.Kernel.Common;

namespace Domain.Aggregates.Inventories
{
    public class InventoryRelation : DefaultEntity<long>
    {
        public long? InventoryDocumentId { get; set; }
        public long? InventoryInvoiceId { get; set; }
        public decimal Amount { get; set; }
        public InventoryDocument InventoryDocument { get; set; } = default!;
        public InventoryInvoice InventoryInvoice { get; set; } = default!;
    }
}
