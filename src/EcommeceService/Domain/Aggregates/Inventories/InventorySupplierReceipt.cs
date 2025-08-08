using Domain.Aggregates.Suppliers;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Inventories
{
    public class InventorySupplierReceipt : DefaultEntity<long>
    {
        public long InventoryDocumentId { get; set; }
        public long SupplierId { get; set; }
        public string PdfUrl { get; set; }
        public InventoryDocument InventoryDocument { get; set; }
        public Supplier Supplier { get; set; }
    }
}
