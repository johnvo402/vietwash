using Domain.Aggregates.Services;
using Domain.Aggregates.Suppliers;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Inventories
{
    public class ProductSupplying : BaseEntity<long>
    {
        public long ProductId { get; set; } = default!;
        public long SupplierId { get; set; } = default!;
        public long InventoryDocumentId { get; set; } = default!;
        public int Quantity { get; set; } = default!;
        public string LotNumber { get; set; } = default!;
        public string Sku { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public short Type { get; set; } = default!;
        public long UnitRelationId { get; set; } = default!;
        public DateTimeOffset ExperyDate { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ArriveAt { get; set; } = DateTimeOffset.UtcNow;
        public UnitRelation UnitRelation { get; set; } = default!;
        public Supplier Suppliers { get; set; } = default!;
        public InventoryDocument InventoryDocuments { get; set; } = default!;
    }
}
