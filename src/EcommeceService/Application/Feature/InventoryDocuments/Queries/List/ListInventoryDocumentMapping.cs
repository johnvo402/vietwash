using System.Linq.Expressions;
using Application.Feature.Common.Mapping.Inventories;
using Domain.Aggregates.Inventories;

namespace Application.Feature.InventoryDocuments.Queries.List
{
    public static class ListInventoryDocumentMapping
    {
        public static Expression<
            Func<InventoryDocument, ListInventoryDocumentResponse>
        > Selector() =>
            inventoryDoc =>
                (ListInventoryDocumentResponse)inventoryDoc.ToInventoryDocumentProjection();
    }
}
