using Domain.Aggregates.Inventories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.InventoryImports
{
    public class EquipmentImportItem
    {
        public long? EquipmentSupplyingId { get; set; }
        public long? EquipmentId { get; set; }
        public long SupplierId { get; set; }
        public long UnitRelationId { get; set; } = default!;
        public InventoryDocumentType Type { get; set; } = default!;
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = default!;
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Capacity { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
