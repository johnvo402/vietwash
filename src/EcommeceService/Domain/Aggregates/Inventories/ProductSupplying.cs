using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using Domain.Aggregates.Suppliers;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Inventories
{
    public class ProductSupplying : DefaultEntity<long>
    {
        public long ProductId { get; set; } = default!;
        public long? SupplierId { get; set; }
        public long InventoryDocumentId { get; set; } = default!;
        public decimal Quantity { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public long UnitRelationId { get; set; } = default!;
        public UnitRelation UnitRelation { get; set; } = default!;
        public Supplier? Supplier { get; set; }
        public BranchProduct Product { get; set; } = default!;
        public InventoryDocument InventoryDocument { get; set; } = default!;
    }
}
