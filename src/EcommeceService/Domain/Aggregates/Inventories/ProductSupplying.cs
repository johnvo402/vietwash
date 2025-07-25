using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using Domain.Aggregates.Suppliers;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Inventories
{
    public class ProductSupplying : DefaultEntity<long>
    {
        public long ProductId { get; set; } = default!;
        public long SupplierId { get; set; } = default!;
        public long InventoryDocumentId { get; set; } = default!;
        public int Quantity { get; set; } = default!;
        public string LotNumber { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public long UnitRelationId { get; set; } = default!;
        public DateTimeOffset? ExpiryDate { get; set; }
        public UnitRelation UnitRelation { get; set; } = default!;
        public Supplier Supplier { get; set; } = default!;
        public BranchProduct Product { get; set; } = default!;
        public InventoryDocument InventoryDocument { get; set; } = default!;
    }
}
