using Domain.Aggregates.Inventories;
using Domain.Aggregates.Services;
using Domain.Aggregates.Suppliers;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Equipments;

public class EquipmentSupplying : DefaultEntity
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public decimal Price { get; set; } = default!;
    public decimal Capacity { get; set; } = default!;
    public short Type { get; set; } = default!;
    public DateTimeOffset ExpiryDate { get; set; } = default!;
    public string ArrivedAt { get; set; } = default!;
    public long UnitId { get; set; } = default!;
    public long SupplierId { get; set; } = default!;
    public long InventoryDocumentId { get; set; } = default!;
    public Supplier? Supplier { get; set; }
    public Unit? Unit { get; set; }
    public InventoryDocument? InventoryDocument { get; set; }
}
