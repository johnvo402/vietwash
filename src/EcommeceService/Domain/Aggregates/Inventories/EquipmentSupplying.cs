using Domain.Aggregates.Suppliers;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Inventories;

public class EquipmentSupplying : DefaultEntity<long>
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public decimal Price { get; set; } = default!;
    public decimal Capacity { get; set; } = default!;
    public int Quantity { get; set; } = default!;
    public long SupplierId { get; set; } = default!;
    public long InventoryDocumentId { get; set; } = default!;
    public Supplier Supplier { get; set; } = default!;
    public InventoryDocument InventoryDocument { get; set; } = default!;
}
