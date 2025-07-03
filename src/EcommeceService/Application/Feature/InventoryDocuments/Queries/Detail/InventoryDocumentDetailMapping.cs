using Domain.Aggregates.Inventories;

namespace Application.Feature.InventoryDocuments.Queries.Detail
{
    public static class InventoryDocumentDetailMapping
    {
        public static InventoryDocumentDetailResponse ToInventoryDocumentDetailResponse(
            this InventoryDocument inventoryDocument
        )
        {
            var response = new InventoryDocumentDetailResponse();
            response.MappingFrom(inventoryDocument);
            return response;
        }
    }
}
