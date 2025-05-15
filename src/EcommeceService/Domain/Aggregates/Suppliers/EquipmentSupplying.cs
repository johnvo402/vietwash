using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Suppliers
{
    public class EquipmentSupplying : BaseEntity<long>
    {
        public long SupplierId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public long InventoryDocumentId { get; set; } = default!;
        public string Sku { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public decimal Capacity { get; set; } = default!;
        public short Type { get; set; } = default!;
        public long UnitId { get; set; } = default!;
        public DateTimeOffset ExperyDate { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ArriveAt { get; set; } = DateTimeOffset.UtcNow;
    }
}