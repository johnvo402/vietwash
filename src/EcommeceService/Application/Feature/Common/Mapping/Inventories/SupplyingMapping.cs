using Application.Feature.Common.Projections.Inventories;
using Contracts.Extensions;
using Domain.Aggregates.Inventories;

namespace Application.Feature.Common.Mapping.Inventories
{
    public static class SupplyingMapping
    {
        public static ICollection<ProductSupplying>? ToListProductSupplying(
            this ICollection<ProductSupplyingModel>? productSupplyings
        ) =>
            productSupplyings?.ToListMapping(x => new ProductSupplying
            {
                ProductId = x.ProductId,
                SupplierId = x.SupplierId,
                Quantity = x.Quantity,
                LotNumber = x.LotNumber,
                Sku = x.Sku,
                Price = x.Price,
                UnitRelationId = x.UnitRelationId,
                ExpiryDate = x.ExpiryDate,
            });

        public static ICollection<EquipmentSupplying>? ToListEquipmentSupplying(
            this ICollection<EquipmentSupplyingModel>? equipmentSupplyings
        ) =>
            equipmentSupplyings?.ToListMapping(x => new EquipmentSupplying
            {
                Name = x.Name,
                Code = x.Code,
                Sku = x.Sku,
                Price = x.Price,
                Capacity = x.Capacity,
                UnitRelationId = x.UnitRelationId,
                Quantity = x.Quantity,
                SupplierId = x.SupplierId,
            });

        public static EquipmentSupplyingProjection ToEquipmentSupplyingProjection(
            this EquipmentSupplying equipment
        )
        {
            var response = new EquipmentSupplyingProjection();
            response.MappingFrom(equipment);
            return response;
        }

        public static ProductSupplyingProjection ToProductSupplyingProjection(
            this ProductSupplying supplying
        )
        {
            var response = new ProductSupplyingProjection();
            response.MappingFrom(supplying);
            return response;
        }

        public static ICollection<EquipmentSupplyingProjection> ToListEquipmentSupplyingProjection(
            this ICollection<EquipmentSupplying> equipments
        )
        {
            var response = equipments.Select(x => x.ToEquipmentSupplyingProjection()).ToList();
            return response;
        }

        public static ICollection<ProductSupplyingProjection> ToListProductSupplyingProjection(
            this ICollection<ProductSupplying> productSupplyings
        )
        {
            var response = productSupplyings.Select(x => x.ToProductSupplyingProjection()).ToList();
            return response;
        }
    }
}
