using Domain.Aggregates.Inventories.Enums;

namespace Application.Feature.Suppliers.Query.ImportExportHistory
{
    public class ImportExportHistoryResponse
    {
        public DateTimeOffset? TransactionAt { get; set; }
        public string DocumentCode { get; set; } = null!;
        public decimal Total { get; set; }
        public long InvDocId { get; set; }
        public long? SupplierId { get; set; }
        public Ulid InvDocPublicId { get; set; }
        public InventoryType Type { get; set; }
    }
}
