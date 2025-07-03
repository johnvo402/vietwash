using Application.Feature.Common.Mapping.Services;
using Application.Feature.Common.Mapping.Units;
using Application.Feature.Common.Projections.Suppliers;
using Application.Feature.Common.Projections.Units;
using Contracts.Application.Common;
using Domain.Aggregates.Inventories;

namespace Application.Feature.Common.Projections.Inventories
{
    public class ProductSupplyingProjection : DefaultBaseResponse<long>
    {
        public long ProductId { get; set; } = default!;
        public long SupplierId { get; set; } = default!;
        public long InventoryDocumentId { get; set; } = default!;
        public int Quantity { get; set; } = default!;
        public string LotNumber { get; set; } = default!;
        public string Sku { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public long UnitRelationId { get; set; } = default!;
        public DateTimeOffset? ExpiryDate { get; set; }
        public UnitRelationProjection UnitRelation { get; set; } = default!;
        public SupplierProjection Supplier { get; set; } = default!;

        public virtual void MappingFrom(ProductSupplying supplying)
        {
            Id = supplying.Id;
            ProductId = supplying.ProductId;
            SupplierId = supplying.SupplierId;
            InventoryDocumentId = supplying.InventoryDocumentId;
            Quantity = supplying.Quantity;
            LotNumber = supplying.LotNumber;
            Sku = supplying.Sku;
            Price = supplying.Price;
            UnitRelationId = supplying.UnitRelationId;
            ExpiryDate = supplying.ExpiryDate;
            UnitRelation = supplying.UnitRelation.ToUnitRelationProjectionResponse();
            Supplier = supplying.Supplier.ToSupplierProjection();
        }
    }

    public class EquipmentSupplyingProjection : DefaultBaseResponse<long>
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Sku { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public decimal Capacity { get; set; } = default!;
        public long UnitRelationId { get; set; } = default!;
        public int Quantity { get; set; } = default!;
        public long SupplierId { get; set; } = default!;
        public long InventoryDocumentId { get; set; } = default!;
        public UnitRelationProjection UnitRelation { get; set; } = default!;
        public SupplierProjection Supplier { get; set; } = default!;

        public virtual void MappingFrom(EquipmentSupplying supplying)
        {
            Id = supplying.Id;
            Name = supplying.Name;
            SupplierId = supplying.SupplierId;
            InventoryDocumentId = supplying.InventoryDocumentId;
            Quantity = supplying.Quantity;
            Code = supplying.Code;
            Sku = supplying.Sku;
            Price = supplying.Price;
            UnitRelationId = supplying.UnitRelationId;
            Capacity = supplying.Capacity;

            UnitRelation = supplying.UnitRelation.ToUnitRelationProjectionResponse();
            Supplier = supplying.Supplier.ToSupplierProjection();
        }
    }
}
