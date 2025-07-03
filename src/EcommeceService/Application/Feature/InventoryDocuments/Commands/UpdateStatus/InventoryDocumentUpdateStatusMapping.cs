using Application.Feature.Common.Projections.Inventories;
using Domain.Aggregates.Inventories;

namespace Application.Feature.InventoryDocuments.Commands.UpdateStatus
{
    public static class InventoryDocumentUpdateStatusMapping
    {
        public static void ToUpdateStatusInventoryDocument(
            this InventoryDocument inventoryDocument,
            InventoryDocumentUpdateStatus updateStatus
        ) => inventoryDocument.UpdateStatus(updateStatus.Status, updateStatus.CancelReason);
    }
}
