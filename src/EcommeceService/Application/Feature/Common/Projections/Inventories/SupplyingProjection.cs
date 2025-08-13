using Application.Common.Security;
using Contracts.Application.Common;
using Domain.Aggregates.Inventories;

namespace Application.Feature.Common.Projections.Inventories
{
    public class ProductSupplyingProjection : DefaultBaseResponse<long>
    {
        public long ProductId { get; set; } = default!;
        public string ProductName { get; set; } = default!;
        public long? SupplierId { get; set; }
        public decimal Quantity { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public long UnitRelationId { get; set; } = default!;
        public string UnitName { get; set; } = default!;
        public string? SupplierName { get; set; }

        public virtual void MappingFrom(ProductSupplying supplying)
        {
            Id = supplying.Id;
            ProductId = supplying.ProductId;
            ProductName = supplying.Product.Name;
            SupplierId = supplying.SupplierId;
            Quantity = supplying.Quantity;
            Price = supplying.Price;
            UnitRelationId = supplying.UnitRelationId;
            UnitName = supplying.UnitRelation.Unit?.Name ?? supplying.UnitRelation.Name;
            SupplierName = supplying.Supplier?.Name;
        }
    }

    public class EquipmentSupplyingProjection : DefaultBaseResponse<long>
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;

        [File]
        public string? Image { get; set; }
        public decimal Price { get; set; } = default!;
        public int Quantity { get; set; } = default!;
        public long SupplierId { get; set; } = default!;
        public string SupplierName { get; set; } = default!;

        public virtual void MappingFrom(EquipmentSupplying supplying)
        {
            Id = supplying.Id;
            Name = supplying.Name;
            Image = supplying.Image;
            SupplierId = supplying.SupplierId;
            Quantity = supplying.Quantity;
            Code = supplying.Code;
            Price = supplying.Price;
            SupplierName = supplying.Supplier.Name;
        }
    }
}
