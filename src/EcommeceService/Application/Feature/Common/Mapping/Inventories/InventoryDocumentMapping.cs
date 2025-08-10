using Application.Feature.Common.Projections.Inventories;
using Contracts.Utils;
using Domain.Aggregates.Inventories;

namespace Application.Feature.Common.Mapping.Inventories
{
    public static class InventoryDocumentMapping
    {
        public static InventoryDocumentProjection ToInventoryDocumentProjection(
            this InventoryDocument inventoryDocument
        )
        {
            var response = new InventoryDocumentProjection();
            response.MappingFrom(inventoryDocument);
            return response;
        }

        public static InventoryReceiptModel ToInventoryReceiptModel(
            this InventoryDocument inventoryDocument,
            string createdBy,
            string branchName
        )
        {
            // Validate input
            if (inventoryDocument == null)
                throw new ArgumentNullException(nameof(inventoryDocument));

            // Get SupplierName from the first EquipmentSupplying or ProductSupplying
            string supplierName =
                inventoryDocument.EquipmentSupplyings.FirstOrDefault()?.Supplier?.Name
                ?? inventoryDocument.ProductSupplyings.FirstOrDefault()?.Supplier?.Name
                ?? "--";

            return new InventoryReceiptModel
            {
                Code = inventoryDocument.Code,
                TransactionAt = inventoryDocument.TransactionAt ?? DateTimeOffset.UtcNow,
                BranchName = branchName,
                SupplierName = supplierName,

                CreatedBy = createdBy, // Replace with actual audit field if available
                CreatedAt = DateTimeOffset.UtcNow, // Replace with actual CreatedAt if available
                Amount = inventoryDocument.Amount.FormatCurrency(), // Assuming Amount is in whole units
                AmountInWords = NumberToTextConverter.ToVietnameseCurrencyText(
                    inventoryDocument.Amount
                ), // Utility function
                LogoUrl = "https://example.com/logo.png", // Replace with actual logic
                StampUrl = "https://example.com/stamp.png", // Replace with actual logic
                EquipmentSupplyings = inventoryDocument
                    .EquipmentSupplyings.Select(e => new EquipmentSupplyingReceipt
                    {
                        Name = e.Name,
                        Price = e.Price.FormatCurrency(),
                        Quantity = e.Quantity,
                        Total = (e.Quantity * e.Price).FormatCurrency(),
                    })
                    .ToList(),
                ProductSupplyings = inventoryDocument
                    .ProductSupplyings.Select(p => new ProductSupplyingReceipt
                    {
                        ProductName = p.Product?.Name ?? "--",
                        Quantity = p.Quantity,
                        Price = p.Price.FormatCurrency(),
                        UnitName = p.UnitRelation?.Name ?? "--",
                        Total = (p.Quantity * p.Price).FormatCurrency(),
                    })
                    .ToList(),
            };
        }
    }
}
