using Application.Feature.Common.Projections.Inventories;
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
    }
}
