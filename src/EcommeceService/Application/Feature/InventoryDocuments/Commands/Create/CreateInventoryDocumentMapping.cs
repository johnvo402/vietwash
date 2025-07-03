using Application.Feature.Common.Mapping.Inventories;
using Application.Feature.Common.Projections.Inventories;
using Contracts.Utils;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;

namespace Application.Feature.InventoryDocuments.Commands.Create
{
    public static class CreateInventoryDocumentMapping
    {
        public static InventoryDocument ToEntity(this InventoryDocumentModel model)
        {
            long? fromWarehouseId = null;
            long? toWarehouseId = null;
            string prefixCode = "NH";
            string code = Generator.GenerateCode(prefixCode, 6);

            switch (model.Type)
            {
                case InventoryType.Import:
                    toWarehouseId = model.WarehouseId;
                    break;
                case InventoryType.Export:
                    fromWarehouseId = model.WarehouseId;
                    break;
            }

            var productSupplyings = model.ProductSupplyings.ToListProductSupplying() ?? [];
            var equipmentSupplyings = model.EquipmentSupplyings.ToListEquipmentSupplying() ?? [];

            // ✅ Tính amount ngoài trước
            var amount = CalculateTotalAmount(productSupplyings, equipmentSupplyings);

            // ✅ Tạo entity với amount truyền vào
            var inventoryDocument = new InventoryDocument(
                code: code,
                type: model.Type,
                fromWarehouseId: fromWarehouseId,
                toWarehouseId: toWarehouseId,
                branchId: model.BranchId,
                note: model.Note,
                amount: amount
            );

            // ✅ Gán list
            inventoryDocument.ProductSupplyings = productSupplyings;
            inventoryDocument.EquipmentSupplyings = equipmentSupplyings;

            return inventoryDocument;
        }

        private static decimal CalculateTotalAmount(
            IEnumerable<ProductSupplying> productSupplyings,
            IEnumerable<EquipmentSupplying> equipmentSupplyings
        )
        {
            var productTotal = productSupplyings.Sum(x => x.Price * x.Quantity);
            var equipmentTotal = equipmentSupplyings.Sum(x => x.Price * x.Quantity);
            var total = productTotal + equipmentTotal;

            // Validate
            if (total <= 0)
            {
                total = 0;
            }

            return total;
        }
    }
}
