using Domain.Aggregates.Inventories;
using Domain.Aggregates.Services;
using Domain.Aggregates.Suppliers;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Inventories;

public class EquipmentSupplying : DefaultEntity<long>
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public decimal Price { get; set; } = default!;
    public decimal Capacity { get; set; } = default!;
    public short Type { get; set; } = default!;
    public DateTimeOffset ExpiryDate { get; set; } = default!;
    public string ArrivedAt { get; set; } = default!;
    public long UnitRelationId { get; set; } = default!;
    public long SupplierId { get; set; } = default!;
    public long InventoryDocumentId { get; set; } = default!;
    public Supplier? Supplier { get; set; }
    public UnitRelation UnitRelation { get; set; } = default!;
    public InventoryDocument? InventoryDocument { get; set; }
}
