using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Aggregates.Services;
using Domain.Aggregates.Suppliers;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Suppliers
{
    public class ProductSupplying : BaseEntity<long>
    {
        public long ProductId { get; set; } = default!;
        public long SupplierId { get; set; } = default!;
        public long InvetoryDocumentId { get; set; } = default!;
        public int Quantity { get; set; } = default!;
        public string LotNumber { get; set; } = default!;
        public string Sku { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public short Type { get; set; } = default!;
        public long UnitId { get; set; } = default!;
        public DateTimeOffset ExperyDate { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ArriveAt { get; set; } = DateTimeOffset.UtcNow;

        public Unit Units { get; set; } = default!;
        public Supplier Suppliers { get; set; } = default!;

        //public Product Products { get; set; } = default!;
        //public InventoryDocument InventoryDocuments { get; set; } = default!;
    }
}
