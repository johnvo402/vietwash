using System.Linq.Expressions;
using Domain.Aggregates.Inventories;

namespace Application.Feature.InventoryDocuments.Queries.List
{
    public static class ListInventoryDocumentMapping
    {
        public static Expression<
            Func<InventoryDocument, ListInventoryDocumentResponse>
        > Selector() =>
            document => new ListInventoryDocumentResponse
            {
                Id = document.Id,
                PublicId = document.PublicId,
                CreatedAt = document.CreatedAt,
                CreatedBy = document.CreatedBy,
                UpdatedAt = document.UpdatedAt,
                UpdatedBy = document.UpdatedBy,

                Amount = document.Amount,
                PaidAmount = document.PaidAmount,
                BranchId = document.BranchId,
                TransactionAt = document.TransactionAt,
                Code = document.Code,
                Status = document.Status,
                ArrivedAt = document.ArrivedAt,
                Type = document.Type,
            };
    }
}
